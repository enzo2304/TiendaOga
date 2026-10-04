namespace TiendaOga.Entidades
{
    // Fila que devuelve sp_cliente_listar_con_compras
    public class ClienteRow
    {
        public int IdCliente { get; set; }
        public string TipoCliente { get; set; }        // PERSONA / EMPRESA
        public string NombreRazonSocial { get; set; }
        public string TipoDocumento { get; set; }      // DNI / CUIT / CUIL / PASAPORTE
        public string NroDocumento { get; set; }
        public string Telefono { get; set; }
        public bool Activo { get; set; }
        public int Compras { get; set; }
    }
}