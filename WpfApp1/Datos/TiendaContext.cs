using System;
using System.Data.Entity; // Fundamental para que funcione Entity Framework
using TiendaOga.Tablas;   // Para que reconozca tus clases Usuario y Perfil

namespace TiendaOga.Datos
{
    // Al heredar de DbContext, esta clase se convierte en el puente oficial
    public class TiendaContext : DbContext
    {
        // El constructor llama al método que decide qué conexión usar según la PC
        public TiendaContext() : base(ObtenerCadenaConexion())
        {
        }

        // Le avisamos a Entity Framework qué tablas tiene que manejar
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Perfil> Perfil { get; set; }

        // Método inteligente para evitar el error entre tu PC y la de tu compañero
        private static string ObtenerCadenaConexion()
        {
            string nombrePC = Environment.MachineName;

            // Tu nombre de PC (el que vimos en tu captura de SQL Server)
            if (nombrePC == "DESKTOP-JRPDUIS")
            {
                return @"Server=(localdb)\ProjectModels;Database=TiendaOgaDB;Integrated Security=True;";
            }
            else
            {
                // La conexión de tu compañero (se puede ajustar el nombre de su PC luego si hace falta)
                return @"Server=localhost\MSSQLSERVER01;Database=TiendaOgaDB;Integrated Security=True;";
            }
        }
    }
}