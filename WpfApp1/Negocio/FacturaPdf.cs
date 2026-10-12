using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TiendaOga.Entidades;

namespace TiendaOga.Negocio
{
    public static class DatosEmisor
    {
        public const string Nombre = "TIENDA OGA";
        public const string Subtitulo = "Comprobante de venta";
        public const string Pie = "¡Gracias por su compra!";
    }

    public static class FormatoTicket
    {
        private static readonly CultureInfo Ar = CultureInfo.GetCultureInfo("es-AR");
        public static string Moneda(decimal d) => "$" + d.ToString(d % 1 == 0 ? "N0" : "N2", Ar);
    }

    public static class FacturaPdf
    {
        private static readonly string Guiones = new string('-', 42);

        public static void Generar(Factura f, string ruta)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            QuestPDF.Settings.UseSystemFonts = true;
            Document.Create(doc =>
            {
                doc.Page(page =>
                {
                    page.ContinuousSize(226);   // ancho 80 mm; el alto se ajusta al contenido
                    page.Margin(10);
                    page.DefaultTextStyle(x => x.FontFamily("Courier New").FontSize(8));

                    page.Content().Column(col =>
                    {
                        col.Item().AlignCenter().Text(DatosEmisor.Nombre).FontSize(14).Bold();
                        col.Item().AlignCenter().Text(DatosEmisor.Subtitulo).FontSize(7);
                        col.Item().Text(Guiones);

                        Fila(col, "N° Venta:", f.NumeroVenta);
                        Fila(col, "Fecha:", f.FechaVenta.ToString("dd/MM/yyyy HH:mm"));
                        Fila(col, "Cliente:", f.NombreCliente);
                        Fila(col, "Vendedor:", f.Vendedor);
                        Fila(col, "Pago:", string.Join(", ", f.Pagos.ConvertAll(p => p.TipoPago)));

                        
                        col.Item().Text(Guiones);

                        col.Item().Table(t =>
                        {
                            t.ColumnsDefinition(c =>
                            {
                                c.ConstantColumn(22);
                                c.RelativeColumn();
                                c.ConstantColumn(44);
                                c.ConstantColumn(46);
                            });

                            t.Cell().BorderBottom(0.5f).Text("Cant").Bold();
                            t.Cell().BorderBottom(0.5f).Text("Producto").Bold();
                            t.Cell().BorderBottom(0.5f).AlignRight().Text("Precio").Bold();
                            t.Cell().BorderBottom(0.5f).AlignRight().Text("Subtotal").Bold();

                            foreach (var i in f.Items)
                            {
                                t.Cell().PaddingTop(2).Text(i.Cantidad.ToString());
                                t.Cell().PaddingTop(2).Text(i.Nombre);
                                t.Cell().PaddingTop(2).AlignRight().Text(FormatoTicket.Moneda(i.PrecioUnitario));
                                t.Cell().PaddingTop(2).AlignRight().Text(FormatoTicket.Moneda(i.SubTotal));
                            }
                        });

                        col.Item().PaddingTop(4).BorderTop(0.5f).Row(r =>
                        {
                            r.RelativeItem().Text("TOTAL").FontSize(11).Bold();
                            r.AutoItem().Text(FormatoTicket.Moneda(f.TotalVenta)).FontSize(11).Bold();
                        });

                        col.Item().Text(Guiones);
                        col.Item().AlignCenter().Text(DatosEmisor.Pie);
                        col.Item().AlignCenter().Text("Comprobante no válido como factura").FontSize(7).Italic();
                    });
                });
            }).GeneratePdf(ruta);
        }

        private static void Fila(ColumnDescriptor col, string etiqueta, string valor)
        {
            col.Item().Row(r =>
            {
                r.AutoItem().Text(etiqueta + " ");
                r.RelativeItem().AlignRight().Text(valor);
            });
        }
    }
}