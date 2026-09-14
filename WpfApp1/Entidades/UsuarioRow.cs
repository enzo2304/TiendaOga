namespace TiendaOga.Entidades
{
    /// <summary>
    /// Representa una fila de la grilla de usuarios (incluye el nombre
    /// y el id del perfil, resueltos por el JOIN).
    /// </summary>
    public class UsuarioRow
    {
        public int IdUsuario { get; set; }
        public int IdPerfil { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string UsuarioLogin { get; set; }
        public string Email { get; set; }
        public string NombrePerfil { get; set; }
        public bool Activo { get; set; }
    }
}