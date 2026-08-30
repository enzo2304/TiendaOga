namespace TiendaOga.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<TiendaOga.Datos.TiendaContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(TiendaOga.Datos.TiendaContext context)
        {
            // 1. Creamos el Perfil Administrador (AddOrUpdate evita que se duplique si ejecutamos el comando varias veces)
            context.Perfil.AddOrUpdate(
                p => p.nombre_perfil,
                new TiendaOga.Tablas.Perfil
                {
                    nombre_perfil = "Administrador",
                    descripcion = "Acceso total al sistema",
                    Activo = true
                },
                new TiendaOga.Tablas.Perfil
                {
                    nombre_perfil = "Gerente",
                    descripcion = "Acceso a reportes y gestión",
                    Activo = true
                },
                new TiendaOga.Tablas.Perfil
                {
                    nombre_perfil = "Vendedor",
                    descripcion = "Acceso a ventas y clientes",
                    Activo = true
                }
            );

            // Guardamos los cambios para que se genere el id_perfil en la base de datos
            context.SaveChanges();

            // 2. Buscamos el ID de ese perfil que acabamos de crear
            var perfilAdmin = context.Perfil.FirstOrDefault(p => p.nombre_perfil == "Administrador");

            // 3. Creamos tu usuario administrador base asignándole ese ID de perfil
            if (perfilAdmin != null)
            {
                context.Usuarios.AddOrUpdate(
                    u => u.usuario, // Evita duplicar el usuario "admin"
                    new TiendaOga.Tablas.Usuario
                    {
                        id_perfil = perfilAdmin.id_perfil,
                        nombre = "Juan",
                        apellido = "Perez",
                        usuario = "admin",
                        password = "123", // Contraseña base para pruebas
                        email = "admin@tiendaoga.com",
                        Activo = true
                    }
                );
            }
        }
    }
}
