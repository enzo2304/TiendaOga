using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TiendaOga.Entidades
{
    public class ProductoRow : INotifyPropertyChanged
    {
        private bool _activo = true;

        public int IdProducto { get; set; }
        public string NombreProducto { get; set; }
        public string NombreCategoria { get; set; }
        public decimal PrecioCosto { get; set; }
        public decimal PrecioVentas { get; set; }
        public int Stock { get; set; }
        public string TipoProducto { get; set; } // "Hogar" o "Tecnologia"

        // Baja lógica: al cambiar esto, la grilla se actualiza sola (fila roja / ícono de reactivar)
        public bool Activo
        {
            get => _activo;
            set { _activo = value; OnPropertyChanged(); }
        }

        // Propiedad de lectura para el selector de Ventas
        public string NombreCompletoDisplay => $"[{IdProducto}] {NombreProducto} (Stock: {Stock})";

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}