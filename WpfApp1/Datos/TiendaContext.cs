using System;
using System.Data.Entity; // Fundamental para que funcione Entity Framework
using TiendaOga.Tablas;   // Para que reconozca tus clases Usuario y Perfil

namespace TiendaOga.Datos
{
    // Al heredar de DbContext, esta clase se convierte en el puente oficial
    public class TiendaContext : DbContext
    {
        // El constructor ahora lee la conexión desde App.config (sección <connectionStrings>)
        // Cada PC (la tuya, la de tu compañero, cualquier otra) define su propia
        // cadena en su App.config local, sin tocar este código.
        public TiendaContext() : base("name=TiendaConnection")
        {
        }

        // Le avisamos a Entity Framework qué tablas tiene que manejar
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Perfil> Perfil { get; set; }
    }
}