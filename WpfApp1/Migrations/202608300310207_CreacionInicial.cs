namespace TiendaOga.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CreacionInicial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Perfil",
                c => new
                    {
                        id_perfil = c.Int(nullable: false, identity: true),
                        nombre_perfil = c.String(),
                        descripcion = c.String(),
                        Activo = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.id_perfil);
            
            CreateTable(
                "dbo.Usuario",
                c => new
                    {
                        id_usuario = c.Int(nullable: false, identity: true),
                        id_perfil = c.Int(nullable: false),
                        nombre = c.String(),
                        apellido = c.String(),
                        usuario = c.String(),
                        password = c.String(),
                        email = c.String(),
                        Activo = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.id_usuario);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Usuario");
            DropTable("dbo.Perfil");
        }
    }
}
