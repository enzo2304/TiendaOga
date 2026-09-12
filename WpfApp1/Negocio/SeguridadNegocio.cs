namespace TiendaOga.Negocio
{
    /// <summary>
    /// Encapsula el hasheo y verificación de contraseñas con BCrypt.
    /// Nunca se guarda ni se compara la contraseña en texto plano.
    ///
    /// Uso:
    ///  - Al CREAR o CAMBIAR una contraseña: HashPassword(passwordPlano) y guardar
    ///    ese resultado en la columna `password` de Usuario.
    ///  - Al hacer LOGIN: traer el hash guardado en la base y llamar a
    ///    VerificarPassword(passwordIngresado, hashGuardado).
    /// </summary>
    public static class SeguridadNegocio
    {
        public static string HashPassword(string passwordPlano)
        {
            return BCrypt.Net.BCrypt.HashPassword(passwordPlano);
        }

        public static bool VerificarPassword(string passwordPlano, string hashGuardado)
        {
            return BCrypt.Net.BCrypt.Verify(passwordPlano, hashGuardado);
        }
    }
}
