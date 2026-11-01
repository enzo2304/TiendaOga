using System;
using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TiendaOga.Datos;
using TiendaOga.Entidades;

namespace TiendaOga.Negocio
{
    public static class NotaCreditoNegocio
    {
        public static NotaCredito Obtener(int idVenta)
        {
            if (idVenta <= 0) throw new ArgumentException("Venta inválida.");

            Factura venta = FacturaNegocio.Obtener(idVenta);
            if (venta == null)
                throw new InvalidOperationException("La venta no existe.");

            if (!new NotaCreditoDatos().ObtenerExtra(idVenta, out DateTime fecha, out string anulador))
                throw new InvalidOperationException("La venta no está anulada, por lo que no tiene nota de crédito.");

            string nv = venta.NumeroVenta ?? idVenta.ToString("000000");
            string numeroNC = nv.StartsWith("V-") ? "NC-" + nv.Substring(2) : "NC-" + nv;

            return new NotaCredito
            {
                NumeroNC = numeroNC,
                Venta = venta,
                FechaAnulacion = fecha,
                Anulador = anulador
            };
        }

        public static void GuardarPdf(NotaCredito nc, string ruta)
        {
            NotaCreditoPdf.Generar(nc, ruta);
        }
    }

    // Nota de crédito en formato formal A4 (basado en el modelo de nota de crédito clásico)
    public static class NotaCreditoPdf
    {
        private const int FilasMinimas = 14;
        private static readonly string ColorLinea = Colors.Grey.Darken2;

        public static void Generar(NotaCredito nc, string ruta)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            QuestPDF.Settings.UseSystemFonts = true;

            var f = nc.Venta;
            string pago = string.Join(", ", f.Pagos.ConvertAll(p => p.TipoPago));

            Document.Create(doc =>
            {
                doc.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(10));

                    page.Content().Border(1).BorderColor(ColorLinea).Column(col =>
                    {
                        // ---------- Encabezado ----------
                        col.Item().BorderBottom(1).BorderColor(ColorLinea).Height(105).Row(r =>
                        {
                            r.RelativeItem(5).Padding(14).Column(c =>
                            {
                                c.Item().Text(DatosEmisor.Nombre).FontSize(24).Bold();
                                c.Item().Text("Nota de crédito por anulación de venta").FontSize(9);
                            });

                            r.ConstantItem(90).PaddingTop(0).Column(c =>
                            {
                                c.Item().AlignCenter().Width(40).Height(40).Border(1).BorderColor(ColorLinea)
                                    .AlignCenter().AlignMiddle().Text("X").FontSize(26).Bold();
                                c.Item().AlignCenter().PaddingTop(2).Text("Documento no\nválido como\nfactura").FontSize(6).AlignCenter();
                            });

                            r.RelativeItem(6).Padding(14).Column(c =>
                            {
                                c.Item().Text("NOTA DE CRÉDITO").FontSize(15);
                                c.Item().PaddingTop(4).Text("Nº " + nc.NumeroNC).FontSize(20);
                                c.Item().PaddingTop(8).Row(rf =>
                                {
                                    rf.Spacing(6);
                                    rf.AutoItem().AlignMiddle().Text("Fecha").FontSize(11);
                                    Caja(rf, nc.FechaAnulacion.ToString("dd"));
                                    Caja(rf, nc.FechaAnulacion.ToString("MM"));
                                    Caja(rf, nc.FechaAnulacion.ToString("yyyy"));
                                });
                            });
                        });

                        // ---------- Datos del cliente / venta ----------
                        col.Item().BorderBottom(1).BorderColor(ColorLinea).Padding(10).Column(c =>
                        {
                            c.Spacing(5);
                            c.Item().Row(r =>
                            {
                                r.RelativeItem(3).Text(t => { t.Span("Señor(es): ").SemiBold(); t.Span(f.NombreCliente); });
                                r.RelativeItem(2).Text(t => { t.Span((f.TipoDocumento ?? "Doc.") + ": ").SemiBold(); t.Span(f.NroDocumento ?? "-"); });
                            });
                            c.Item().Row(r =>
                            {
                                r.RelativeItem(3).Text(t => { t.Span("Venta original: ").SemiBold(); t.Span(f.NumeroVenta); });
                                r.RelativeItem(2).Text(t => { t.Span("Fecha de venta: ").SemiBold(); t.Span(f.FechaVenta.ToString("dd/MM/yyyy")); });
                            });
                            c.Item().Row(r =>
                            {
                                r.RelativeItem(3).Text(t => { t.Span("Vendedor: ").SemiBold(); t.Span(f.Vendedor); });
                                r.RelativeItem(2).Text(t => { t.Span("Anulado por: ").SemiBold(); t.Span(nc.Anulador); });
                            });
                            c.Item().Text(t => { t.Span("Pago original: ").SemiBold(); t.Span(pago); });
                        });

                        // ---------- Detalle ----------
                        col.Item().Table(t =>
                        {
                            t.ColumnsDefinition(c =>
                            {
                                c.ConstantColumn(62);
                                c.RelativeColumn();
                                c.ConstantColumn(90);
                                c.ConstantColumn(90);
                            });

                            t.Cell().Element(Celda).AlignCenter().Text("Cantidad");
                            t.Cell().Element(Celda).AlignCenter().Text("D e s c r i p c i ó n");
                            t.Cell().Element(Celda).AlignCenter().Text("P. Unitario");
                            t.Cell().Element(Celda).AlignCenter().Text("Importe");

                            foreach (var i in f.Items)
                            {
                                t.Cell().Element(Celda).AlignCenter().Text(i.Cantidad.ToString());
                                t.Cell().Element(Celda).Text(i.Nombre);
                                t.Cell().Element(Celda).AlignRight().Text(FormatoTicket.Moneda(i.PrecioUnitario));
                                t.Cell().Element(Celda).AlignRight().Text(FormatoTicket.Moneda(i.SubTotal));
                            }

                            for (int k = f.Items.Count; k < FilasMinimas; k++)
                            {
                                t.Cell().Element(Celda).Text(" ");
                                t.Cell().Element(Celda).Text(" ");
                                t.Cell().Element(Celda).Text(" ");
                                t.Cell().Element(Celda).Text(" ");
                            }
                        });

                        // ---------- Totales ----------
                        col.Item().BorderTop(1).BorderColor(ColorLinea).Column(c =>
                        {
                            c.Item().Row(r =>
                            {
                                r.RelativeItem();
                                r.ConstantItem(90).Padding(5).AlignRight().Text("Subtotal  $");
                                r.ConstantItem(90).Border(0.5f).BorderColor(ColorLinea).Padding(4).AlignRight()
                                    .Text(FormatoTicket.Moneda(f.TotalVenta));
                            });
                            c.Item().Row(r =>
                            {
                                r.RelativeItem();
                                r.ConstantItem(90).Padding(5).AlignRight().Text("TOTAL  $").Bold();
                                r.ConstantItem(90).Border(0.5f).BorderColor(ColorLinea).Padding(4).AlignRight()
                                    .Text(FormatoTicket.Moneda(f.TotalVenta)).Bold();
                            });
                        });

                        // ---------- Pie ----------
                        col.Item().BorderTop(1).BorderColor(ColorLinea).Padding(12).MinHeight(60).Column(c =>
                        {
                            c.Item().Text("Se devolvió el stock de los productos al inventario.");
                            c.Item().PaddingTop(4).Text("Comprobante no fiscal. No válido como factura.").FontSize(8).Italic();
                        });
                    });
                });
            }).GeneratePdf(ruta);
        }

        private static void Caja(RowDescriptor r, string texto)
        {
            r.ConstantItem(38).Height(22).Border(1).BorderColor(ColorLinea)
                .AlignCenter().AlignMiddle().Text(texto);
        }

        private static IContainer Celda(IContainer c)
        {
            return c.Border(0.5f).BorderColor(Colors.Grey.Medium).PaddingVertical(4).PaddingHorizontal(5).MinHeight(20);
        }
    }
}