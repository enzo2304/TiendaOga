using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace TiendaOga.Vistas
{
    public partial class Backup : Page
    {
        public Backup()
        {
            InitializeComponent();
        }

        private void btnExaminar_Click(object sender, RoutedEventArgs e)
        {
            var dialogo = new SaveFileDialog
            {
                Title = "Elegir dónde guardar el backup",
                Filter = "Archivos de texto (*.txt)|*.txt|Todos los archivos (*.*)|*.*",
                FileName = "TiendaOga_DB",
                DefaultExt = ".txt"
            };

            if (dialogo.ShowDialog() == true)
            {
                txtRuta.Text = dialogo.FileName;
            }
        }

        private void btnGenerarBackup_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtRuta.Text))
            {
                MessageBox.Show("Elegí primero una ruta con el botón \"Examinar...\".",
                    "Falta la ruta de destino", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Prueba estática: acá es donde después va a ir la lógica real
                // (mysqldump, backup de SQL Server, copia del .mdf, etc.).
                var contenido = $"Este es un backup de prueba generado el {DateTime.Now:dd/MM/yyyy HH:mm:ss}";
                File.WriteAllText(txtRuta.Text, contenido);

                MessageBox.Show("La copia de seguridad se generó correctamente.",
                    "Backup generado", MessageBoxButton.OK, MessageBoxImage.Information);

                txtRuta.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo generar el backup:\n{ex.Message}",
                    "Error al generar el backup", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}