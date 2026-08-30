namespace TiendaOga.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AgregarForeignKeyPerfil : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.Usuario", "id_perfil");
            AddForeignKey("dbo.Usuario", "id_perfil", "dbo.Perfil", "id_perfil", cascadeDelete: false);
        }

        public override void Down()
        {
            DropForeignKey("dbo.Usuario", "id_perfil", "dbo.Perfil");
            DropIndex("dbo.Usuario", new[] { "id_perfil" });
        }

    }
}
