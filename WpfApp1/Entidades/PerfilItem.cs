namespace TiendaOga.Entidades
{
    /// <summary>
    /// Representa un perfil disponible para elegir en un ComboBox.
    /// Se usa solo para mostrar/seleccionar, no es el mapeo completo de la tabla Perfiles.
    /// </summary>
    public class PerfilItem
    {
        public int IdPerfil { get; set; }
        public string NombrePerfil { get; set; }
    }
}
