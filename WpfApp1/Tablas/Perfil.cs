using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TiendaOga.Tablas
{
    [Table("Perfil")]
    public class Perfil
    {
        [Key]
        public int id_perfil { get; set; }

        public string nombre_perfil { get; set; }

        public string descripcion { get; set; }

        public bool Activo { get; set; }
    } 
} 