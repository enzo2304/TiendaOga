using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace TiendaOga.Vistas
{
    public partial class GuiaOverlay : UserControl
    {
        private List<PasoGuia> _pasos = new List<PasoGuia>();
        private int _indice;

        public GuiaOverlay()
        {
            InitializeComponent();
            PreviewKeyDown += GuiaOverlay_PreviewKeyDown;
        }

        public bool EstaActiva
        {
            get { return Visibility == Visibility.Visible; }
        }

        public void Iniciar(List<PasoGuia> pasos)
        {
            if (pasos == null || pasos.Count == 0) return;

            _pasos = pasos;
            _indice = 0;
            Visibility = Visibility.Visible;
            Focus();
            Keyboard.Focus(this);
            MostrarPaso();
        }

        public void Cerrar()
        {
            Visibility = Visibility.Collapsed;
            _pasos = new List<PasoGuia>();
        }

        private void MostrarPaso()
        {
            PasoGuia paso = _pasos[_indice];

            txtProgreso.Text = string.Format("{0} / {1} · {2}", _indice + 1, _pasos.Count,
                                             (paso.Capitulo ?? string.Empty).ToUpperInvariant());
            txtTitulo.Text = paso.Titulo;
            txtTexto.Text = paso.Texto;
            btnAnterior.IsEnabled = _indice > 0;
            btnSiguiente.Content = (_indice == _pasos.Count - 1) ? "Terminar" : "Siguiente";

            // Si el elemento está dentro de un ScrollViewer, lo trae a la vista antes de medirlo
            if (paso.Elemento != null) paso.Elemento.BringIntoView();

            Dispatcher.BeginInvoke(new Action(Posicionar), DispatcherPriority.Loaded);
        }

        private void Posicionar()
        {
            if (!EstaActiva || _pasos.Count == 0) return;

            double W = raiz.ActualWidth, H = raiz.ActualHeight;
            if (W <= 0 || H <= 0) return;

            FrameworkElement el = _pasos[_indice].Elemento;
            Rect hueco = Rect.Empty;

            if (el != null && el.IsVisible && el.ActualWidth > 0 && el.ActualHeight > 0)
            {
                try
                {
                    Point p = el.TranslatePoint(new Point(0, 0), raiz);
                    hueco = new Rect(p.X, p.Y, el.ActualWidth, el.ActualHeight);
                    hueco.Inflate(6, 6);
                    hueco.Intersect(new Rect(0, 0, W, H));
                }
                catch (InvalidOperationException)
                {
                    hueco = Rect.Empty;
                }
            }

            // Fondo oscuro con hueco
            var todo = new RectangleGeometry(new Rect(0, 0, W, H));
            if (hueco.IsEmpty)
            {
                fondo.Data = todo;
                marco.Visibility = Visibility.Collapsed;
            }
            else
            {
                fondo.Data = new CombinedGeometry(GeometryCombineMode.Exclude, todo,
                                                  new RectangleGeometry(hueco, 6, 6));
                marco.Margin = new Thickness(hueco.X, hueco.Y, 0, 0);
                marco.Width = hueco.Width;
                marco.Height = hueco.Height;
                marco.Visibility = Visibility.Visible;
            }

            // Tarjeta: debajo del elemento, si no entra arriba, a la derecha o a la izquierda
            tarjeta.Margin = new Thickness(0);
            tarjeta.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Size t = tarjeta.DesiredSize;
            const double sep = 14;
            double x, y;

            if (hueco.IsEmpty)
            {
                x = (W - t.Width) / 2;
                y = (H - t.Height) / 2;
            }
            else if (hueco.Bottom + sep + t.Height <= H)
            {
                x = hueco.X; y = hueco.Bottom + sep;
            }
            else if (hueco.Top - sep - t.Height >= 0)
            {
                x = hueco.X; y = hueco.Top - sep - t.Height;
            }
            else if (hueco.Right + sep + t.Width <= W)
            {
                x = hueco.Right + sep; y = hueco.Y;
            }
            else if (hueco.Left - sep - t.Width >= 0)
            {
                x = hueco.Left - sep - t.Width; y = hueco.Y;
            }
            else
            {
                x = (W - t.Width) / 2; y = H - t.Height - 20;
            }

            x = Math.Max(12, Math.Min(x, W - t.Width - 12));
            y = Math.Max(12, Math.Min(y, H - t.Height - 12));
            tarjeta.Margin = new Thickness(x, y, 0, 0);
        }

        // ---------------- Eventos ----------------
        private void Raiz_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Posicionar();
        }

        private void Siguiente_Click(object sender, RoutedEventArgs e)
        {
            if (_indice >= _pasos.Count - 1) { Cerrar(); return; }
            _indice++;
            MostrarPaso();
        }

        private void Anterior_Click(object sender, RoutedEventArgs e)
        {
            if (_indice <= 0) return;
            _indice--;
            MostrarPaso();
        }

        private void Cerrar_Click(object sender, RoutedEventArgs e)
        {
            Cerrar();
        }

        private void GuiaOverlay_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!EstaActiva) return;

            switch (e.Key)
            {
                case Key.Escape:
                    Cerrar(); e.Handled = true; break;
                case Key.Right:
                case Key.Enter:
                    Siguiente_Click(null, null); e.Handled = true; break;
                case Key.Left:
                    Anterior_Click(null, null); e.Handled = true; break;
            }
        }
    }
}