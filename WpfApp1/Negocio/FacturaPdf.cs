using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TiendaOga.Entidades;



//genera el documento.
namespace TiendaOga.Negocio
{
    public static class DatosEmisor
    {
        public const string RazonSocial = "TIENDA OGA";
        public const string Domicilio = "Av. Ejemplo 1234 - Corrientes";
        public const string Cuit = "20-12345678-9";
        public const string CondicionIva = "Responsable Monotributo";
        public const string InicioActividades = "01/03/2020";
    }

    public static class FacturaPdf
    {
        private static readonly CultureInfo Ar = CultureInfo.GetCultureInfo("es-AR");

        public static void Generar(Factura f, string rutaArchivo)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            Document.Create(doc =>
            {
                doc.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Content().Column(col =>
                    {
                        col.Spacing(8);

                        // Encabezado: emisor | letra | datos del comprobante
                        col.Item().Border(1).Row(row =>
                        {
                            row.RelativeItem().Padding(8).Column(c =>
                            {
                                c.Item().Text(DatosEmisor.RazonSocial).FontSize(16).Bold();
                                c.Item().Text("Domicilio: " + DatosEmisor.Domicilio);
                                c.Item().Text("Condición IVA: " + DatosEmisor.CondicionIva);
                            });

                            row.ConstantItem(60).BorderLeft(1).BorderRight(1)
                               .AlignCenter().AlignMiddle()
                               .Text("X").FontSize(32).Bold();

                            row.RelativeItem().Padding(8).Column(c =>
                            {
                                c.Item().Text("COMPROBANTE DE VENTA").FontSize(13).Bold();
                                c.Item().Text("N°: " + f.NumeroVenta);
                                c.Item().Text("Fecha: " + f.FechaVenta.ToString("dd/MM/yyyy HH:mm"));
                                c.Item().Text("CUIT: " + DatosEmisor.Cuit);
                                c.Item().Text("Inicio de actividades: " + DatosEmisor.InicioActividades);
                            });
                        });

                        // Cliente y vendedor
                        col.Item().Border(1).Padding(8).Column(c =>
                        {
                            c.Item().Text("Cliente: " + f.NombreCliente);
                            c.Item().Text(f.TipoDocumento + ": " + f.NroDocumento);
                            c.Item().Text("Vendedor: " + f.Vendedor);
                        });

                        // Detalle
                        col.Item().Table(t =>
                        {
                            t.ColumnsDefinition(cd =>
                            {
                                cd.ConstantColumn(50);
                                cd.RelativeColumn();
                                cd.ConstantColumn(60);
                                cd.ConstantColumn(85);
                                cd.ConstantColumn(85);
                            });

                            t.Header(h =>
                            {
                                h.Cell().Element(Encabezado).Text("Código");
                                h.Cell().Element(Encabezado).Text("Descripción");
                                h.Cell().Element(Encabezado).AlignRight().Text("Cant.");
                                h.Cell().Element(Encabezado).AlignRight().Text("P. Unit.");
                                h.Cell().Element(Encabezado).AlignRight().Text("Total");
                            });

                            foreach (var i in f.Items)
                            {
                                t.Cell().Element(Celda).Text(i.IdProducto.ToString());
                                t.Cell().Element(Celda).Text(i.Nombre);
                                t.Cell().Element(Celda).AlignRight().Text(i.Cantidad.ToString());
                                t.Cell().Element(Celda).AlignRight().Text(i.PrecioUnitario.ToString("C2", Ar));
                                t.Cell().Element(Celda).AlignRight().Text(i.SubTotal.ToString("C2", Ar));
                            }
                        });

                        // Pagos y total
                        col.Item().PaddingTop(10).Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Pagos").Bold();
                                foreach (var p in f.Pagos)
                                    c.Item().Text(p.TipoPago + ": " + p.Monto.ToString("C2", Ar));
                            });

                            row.RelativeItem().AlignRight()
                               .Text("Importe total: " + f.TotalVenta.ToString("C2", Ar))
                               .FontSize(14).Bold();
                        });
                    });

                    page.Footer().AlignCenter()
                        .Text("Documento no válido como factura").FontSize(9).Italic();
                });
            }).GeneratePdf(rutaArchivo);
        }

        private static IContainer Encabezado(IContainer c) =>
            c.Background(Colors.Grey.Lighten3).BorderBottom(1).Padding(4).DefaultTextStyle(x => x.Bold());

        private static IContainer Celda(IContainer c) =>
            c.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(4);
    }
}