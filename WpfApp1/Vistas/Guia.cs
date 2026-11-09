using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace TiendaOga.Vistas
{
    /// <summary>Un paso del recorrido guiado.</summary>
    public class PasoGuia
    {
        public FrameworkElement Elemento { get; set; }
        public string Capitulo { get; set; }
        public string Titulo { get; set; }
        public string Texto { get; set; }
        public int Orden { get; set; }
    }

    /// <summary>
    /// Propiedades adjuntas para anotar la ayuda directamente en el XAML:
    ///   ayuda:Guia.Orden="1"  ayuda:Guia.Titulo="..."  ayuda:Guia.Texto="..."
    /// y en la raíz de cada Page: ayuda:Guia.Capitulo="Nombre de la pantalla".
    /// Solo participan los elementos con Orden mayor a 0.
    /// </summary>
    public static class Guia
    {
        // ---------- Orden (0 = el elemento no participa) ----------
        public static readonly DependencyProperty OrdenProperty =
            DependencyProperty.RegisterAttached("Orden", typeof(int), typeof(Guia), new PropertyMetadata(0));
        public static int GetOrden(DependencyObject o) { return (int)o.GetValue(OrdenProperty); }
        public static void SetOrden(DependencyObject o, int v) { o.SetValue(OrdenProperty, v); }

        // ---------- Título del paso ----------
        public static readonly DependencyProperty TituloProperty =
            DependencyProperty.RegisterAttached("Titulo", typeof(string), typeof(Guia), new PropertyMetadata(string.Empty));
        public static string GetTitulo(DependencyObject o) { return (string)o.GetValue(TituloProperty); }
        public static void SetTitulo(DependencyObject o, string v) { o.SetValue(TituloProperty, v); }

        // ---------- Texto del paso ----------
        public static readonly DependencyProperty TextoProperty =
            DependencyProperty.RegisterAttached("Texto", typeof(string), typeof(Guia), new PropertyMetadata(string.Empty));
        public static string GetTexto(DependencyObject o) { return (string)o.GetValue(TextoProperty); }
        public static void SetTexto(DependencyObject o, string v) { o.SetValue(TextoProperty, v); }

        // ---------- Capítulo (se pone en la raíz de la Page) ----------
        public static readonly DependencyProperty CapituloProperty =
            DependencyProperty.RegisterAttached("Capitulo", typeof(string), typeof(Guia), new PropertyMetadata(string.Empty));
        public static string GetCapitulo(DependencyObject o) { return (string)o.GetValue(CapituloProperty); }
        public static void SetCapitulo(DependencyObject o, string v) { o.SetValue(CapituloProperty, v); }

        /// <summary>
        /// Recorre el árbol visual desde la raíz y devuelve los pasos anotados que estén visibles,
        /// ordenados por Orden (a igual Orden, por posición en el XAML).
        /// </summary>
        public static List<PasoGuia> ObtenerPasos(DependencyObject raiz, string capituloPorDefecto)
        {
            var lista = new List<PasoGuia>();
            if (raiz == null) return lista;

            string capitulo = GetCapitulo(raiz);
            if (string.IsNullOrWhiteSpace(capitulo)) capitulo = capituloPorDefecto ?? string.Empty;

            Recolectar(raiz, capitulo, lista);
            return lista.OrderBy(p => p.Orden).ToList();   // OrderBy es estable
        }

        private static void Recolectar(DependencyObject nodo, string capitulo, List<PasoGuia> lista)
        {
            var fe = nodo as FrameworkElement;
            if (fe != null && GetOrden(fe) > 0 && fe.IsVisible && fe.ActualWidth > 0 && fe.ActualHeight > 0)
            {
                lista.Add(new PasoGuia
                {
                    Elemento = fe,
                    Capitulo = capitulo,
                    Titulo = GetTitulo(fe),
                    Texto = GetTexto(fe),
                    Orden = GetOrden(fe)
                });
            }

            int n = VisualTreeHelper.GetChildrenCount(nodo);
            for (int i = 0; i < n; i++)
                Recolectar(VisualTreeHelper.GetChild(nodo, i), capitulo, lista);
        }
    }
}