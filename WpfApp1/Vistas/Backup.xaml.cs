using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using TiendaOga.Entidades;
using TiendaOga.Negocio;

namespace TiendaOga.Vistas
{
    public partial class Backup : Page
    {
        private const string PalabraConfirmacion = "CONFIRMAR";

        private string _nombreSugerido = "";
        private string _archivoRestore = "";

        public Backup()
        {
            InitializeComponent();
            txtBaseOrigen.Text = BackupNegocio.NombreBase;
        }

        private string ObtenerRol()
        {
            var ventana = Window.GetWindow(this) as Window1;
            return ventana != null ? ventana.RolActual : string.Empty;
        }

        private string ObtenerUsuario()
        {
            var ventana = Window.GetWindow(this) as Window1;
            return ventana != null && !string.IsNullOrWhiteSpace(ventana.UsuarioActual)
                ? ventana.UsuarioActual
                : "Desconocido";
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            BackupNegocio.AsegurarCarpeta(BackupNegocio.CarpetaPorDefecto);

            txtCarpeta.Text = BackupNegocio.CarpetaPorDefecto;
            GenerarNombreSugerido();
            CargarHistorial();
            CargarAuditoria();
            await ActualizarEstadoAsync();
        }

        // ====================================================================
        // BACKUP
        // ====================================================================
        private void btnExaminar_Click(object sender, RoutedEventArgs e)
        {
            // Truco para elegir una carpeta sin depender de WinForms
            var dialogo = new OpenFileDialog
            {
                Title = "Elegir carpeta de destino del backup",
                ValidateNames = false,
                CheckFileExists = false,
                CheckPathExists = true,
                FileName = "Seleccionar carpeta",
                InitialDirectory = Directory.Exists(txtCarpeta.Text) ? txtCarpeta.Text : BackupNegocio.CarpetaPorDefecto
            };

            if (dialogo.ShowDialog() == true)
            {
                string carpeta = Path.GetDirectoryName(dialogo.FileName);
                if (!string.IsNullOrWhiteSpace(carpeta))
                {
                    txtCarpeta.Text = carpeta;
                    CargarHistorial();
                }
            }
        }

        private async void btnGenerarBackup_Click(object sender, RoutedEventArgs e)
        {
            string carpeta = txtCarpeta.Text.Trim();
            string nombre = txtNombreArchivo.Text.Trim();
            string rol = ObtenerRol();
            string usuario = ObtenerUsuario();

            // Si no tocó el nombre sugerido, se refresca con la hora actual
            if (nombre == _nombreSugerido)
            {
                GenerarNombreSugerido();
                nombre = txtNombreArchivo.Text;
            }

            string nombreFinal, error;
            if (!BackupNegocio.ValidarGeneracion(rol, carpeta, nombre, out nombreFinal, out error))
            {
                MessageBox.Show(error, "No se puede generar el backup", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (BackupNegocio.ExisteArchivo(carpeta, nombreFinal))
            {
                var r = MessageBox.Show("Ya existe un archivo con ese nombre. ¿Querés reemplazarlo?",
                    "El archivo ya existe", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (r != MessageBoxResult.Yes) return;
            }

            SetOcupado(true, "Generando copia de seguridad...");
            try
            {
                await Task.Run(() => BackupNegocio.Generar(rol, usuario, carpeta, nombreFinal));

                MessageBox.Show($"La copia de seguridad se generó correctamente.\n\nArchivo: {nombreFinal}",
                    "Backup generado", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo generar el backup:\n" + ex.Message +
                    "\n\nSi el error habla de acceso denegado, la cuenta de SQL Server necesita permiso de escritura en la carpeta de destino.",
                    "Error al generar el backup", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SetOcupado(false);
                GenerarNombreSugerido();
                CargarHistorial();
                CargarAuditoria();
                await ActualizarEstadoAsync();
            }
        }

        private void GenerarNombreSugerido()
        {
            _nombreSugerido = BackupNegocio.SugerirNombre();
            txtNombreArchivo.Text = _nombreSugerido;
        }

        // ====================================================================
        // RESTORE
        // ====================================================================
        private void btnBuscarArchivo_Click(object sender, RoutedEventArgs e)
        {
            var dialogo = new OpenFileDialog
            {
                Title = "Buscar archivo de respaldo",
                Filter = "Respaldos de SQL Server (*.bak)|*.bak",
                InitialDirectory = Directory.Exists(txtCarpeta.Text) ? txtCarpeta.Text : BackupNegocio.CarpetaPorDefecto
            };

            if (dialogo.ShowDialog() == true)
                SeleccionarArchivoRestore(dialogo.FileName);
        }

        private void dgHistorial_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = dgHistorial.SelectedItem as BackupArchivo;
            if (item != null)
                SeleccionarArchivoRestore(item.Ruta);
        }

        private void SeleccionarArchivoRestore(string ruta)
        {
            _archivoRestore = ruta;
            txtArchivoRestore.Text = ruta;
            btnRestaurar.IsEnabled = true;
        }

        // Paso 1: valida el archivo y abre la ventana de confirmación (nunca restaura directamente)
        private async void btnRestaurar_Click(object sender, RoutedEventArgs e)
        {
            string archivo = _archivoRestore;
            string rol = ObtenerRol();

            BackupInfo info = null;
            string error = string.Empty;
            int conexiones = -1;

            SetOcupado(true, "Verificando el archivo de respaldo...");
            bool valido = await Task.Run(() =>
            {
                bool ok = BackupNegocio.ValidarRestauracion(rol, archivo, out error, out info);
                if (ok) conexiones = BackupNegocio.ContarConexiones();
                return ok;
            });
            SetOcupado(false);

            if (!valido)
            {
                MessageBox.Show(error, "No se puede restaurar", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            txtModalArchivo.Text =
                $"Archivo a restaurar: {Path.GetFileName(archivo)}\n" +
                $"Creado el {info.FechaBackup:dd/MM/yyyy HH:mm:ss} (SQL Server v{info.VersionMayor}, compatibilidad {info.NivelCompatibilidad}).\n" +
                "Todo lo registrado después de esa fecha se va a perder.";

            if (conexiones > 0)
                txtModalConexiones.Text = $"Hay {conexiones} conexión(es) activa(s) que serán desconectadas.";
            else if (conexiones == 0)
                txtModalConexiones.Text = "No hay otras conexiones activas en este momento.";
            else
                txtModalConexiones.Text = "No se pudo verificar si hay otros usuarios conectados; serán desconectados.";

            txtConfirmar.Clear();
            btnConfirmarRestore.IsEnabled = false;
            contenido.IsEnabled = false;
            overlayConfirm.Visibility = Visibility.Visible;
            txtConfirmar.Focus();
        }

        private void txtConfirmar_TextChanged(object sender, TextChangedEventArgs e)
        {
            btnConfirmarRestore.IsEnabled = txtConfirmar.Text.Trim() == PalabraConfirmacion;
        }

        private void btnCancelarRestore_Click(object sender, RoutedEventArgs e)
        {
            overlayConfirm.Visibility = Visibility.Collapsed;
            contenido.IsEnabled = true;
        }

        // Paso 2: ejecuta la restauración tras la confirmación
        private async void btnConfirmarRestore_Click(object sender, RoutedEventArgs e)
        {
            if (txtConfirmar.Text.Trim() != PalabraConfirmacion) return;

            overlayConfirm.Visibility = Visibility.Collapsed;

            bool backupPrevio = chkBackupPrevio.IsChecked == true;
            string archivo = _archivoRestore;
            string rol = ObtenerRol();
            string usuario = ObtenerUsuario();
            bool restaurado = false;

            SetOcupado(true, "Restaurando base de datos...");
            try
            {
                await Task.Run(() => BackupNegocio.Restaurar(rol, usuario, archivo, backupPrevio));
                restaurado = true;

                MessageBox.Show(
                    "La base de datos se restauró correctamente.\n\nSe cerrará la sesión para cargar los datos restaurados.",
                    "Restauración completada", MessageBoxButton.OK, MessageBoxImage.Information);

                new MainWindow().Show();
                Window.GetWindow(this)?.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo restaurar la base de datos:\n" + ex.Message,
                    "Error al restaurar", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (!restaurado)
                {
                    SetOcupado(false);
                    CargarHistorial();
                    CargarAuditoria();
                    await ActualizarEstadoAsync();
                }
            }
        }

        // ====================================================================
        // ESTADO, HISTORIAL Y AUDITORÍA
        // ====================================================================
        private async Task ActualizarEstadoAsync()
        {
            EstadoBackup estado = await Task.Run(() => BackupNegocio.ObtenerEstado());

            txtUltimoBackup.Text = estado.UltimoBackup == null
                ? "Último respaldo exitoso: todavía no se realizó ninguno"
                : $"Último respaldo exitoso: {Hace(estado.UltimoBackup.Value)} ({estado.UltimoBackup.Value:dd/MM/yyyy HH:mm})";

            txtTamanoBD.Text = estado.TamanioMb.HasValue
                ? $"Tamaño de la base de datos: {estado.TamanioMb.Value:N1} MB"
                : "Tamaño de la base de datos: no disponible";

            SetEstadoConexion(estado.Conectado);
        }

        private void SetEstadoConexion(bool conectado)
        {
            var conv = new BrushConverter();
            var verde = (Brush)conv.ConvertFromString("#2F9E58");
            var rojo = (Brush)conv.ConvertFromString("#C62828");
            var fondoVerde = (Brush)conv.ConvertFromString("#E5F6EA");
            var fondoRojo = (Brush)conv.ConvertFromString("#FDECEA");

            txtEstadoConexion.Text = conectado ? "Conectado" : "Sin conexión";
            txtEstadoConexion.Foreground = conectado ? verde : rojo;
            puntoEstado.Fill = conectado ? verde : rojo;
            badgeEstado.Background = conectado ? fondoVerde : fondoRojo;
        }

        private void CargarHistorial()
        {
            var lista = BackupNegocio.ListarArchivos(txtCarpeta.Text);
            dgHistorial.ItemsSource = lista;
            txtSinRespaldos.Visibility = lista.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void CargarAuditoria()
        {
            dgAuditoria.ItemsSource = BackupNegocio.ListarAuditoria(10);
        }

        // ====================================================================
        // UTILIDADES DE PRESENTACIÓN
        // ====================================================================
        private void SetOcupado(bool ocupado, string mensaje = "Procesando...")
        {
            txtBusy.Text = mensaje;
            overlayBusy.Visibility = ocupado ? Visibility.Visible : Visibility.Collapsed;
            contenido.IsEnabled = !ocupado;
            Mouse.OverrideCursor = ocupado ? Cursors.Wait : null;
        }

        private static string Hace(DateTime fecha)
        {
            TimeSpan d = DateTime.Now - fecha;
            if (d.TotalMinutes < 1) return "hace instantes";
            if (d.TotalMinutes < 60) return $"hace {(int)d.TotalMinutes} min";
            if (d.TotalHours < 24) return $"hace {(int)d.TotalHours} h";
            return $"hace {(int)d.TotalDays} día(s)";
        }
    }
}