using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace TiendaOga.Tablas
{
    [Table("Usuario")]
    public class Usuario
    {
        [Key]
        public int id_usuario { get; set; }
        public int id_perfil { get; set; }
        public string nombre { get; set; }
        public string apellido { get; set; }
        public string usuario { get; set; }
        public string password { get; set; }
        public string email { get; set; }
        public bool Activo { get; set; }

        [ForeignKey("id_perfil")]
        public virtual Perfil Perfil { get; set; } // <- Navegación (FK)
    }
}
