using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using TiendaOga.Datos;
using TiendaOga.Entidades;

namespace TiendaOga.Negocio
{
    public static class ClienteNegocio
    {
        private static readonly Regex RegexNumeros = new Regex(@"^[0-9]+$");
        private static readonly ClienteDatos _datos = new ClienteDatos();

        // Valores permitidos por los CHECK de la tabla cliente
        private static readonly string[] TiposDocumentoValidos = { "DNI", "CUIT", "CUIL", "PASAPORTE" };

        // ---------------------------------------------------------------
        // CONSULTAS
        // ---------------------------------------------------------------

        // Lee de la base y refresca la coleccion en memoria que usa la vista
        public static ObservableCollection<ClienteItem> ObtenerTodos()
        {
            var filas = _datos.Listar();

            DatosGlobales.Clientes.Clear();
            foreach (var fila in filas)
                DatosGlobales.Clientes.Add(ConvertirAItem(fila));

            return DatosGlobales.Clientes;
        }

        public static IEnumerable<ClienteItem> BuscarClientes(string busqueda)
        {
            if (string.IsNullOrWhiteSpace(busqueda))
                return DatosGlobales.Clientes;

            string termino = busqueda.Trim().ToLower();

            return DatosGlobales.Clientes
                .Where(c => c.NombreCompleto.ToLower().Contains(termino) || c.Dni.Contains(termino))
                .ToList();
        }

        // Convierte las filas de la base a CompraCliente, que es lo que ya muestra Clientes.xaml
        public static List<CompraCliente> ObtenerHistorial(int idCliente)
        {
            return _datos.ObtenerHistorial(idCliente)
                .Select(c => new CompraCliente
                {
                    NroComprobante = c.NumeroVenta,
                    Fecha = c.FechaVenta.ToString("dd/MM/yyyy HH:mm"),
                    DetalleProductos = c.Articulos,
                    MetodoPago = c.MetodoPago,
                    Total = c.TotalVenta
                })
                .ToList();
        }

        // ---------------------------------------------------------------
        // VALIDACIONES
        // ---------------------------------------------------------------
        public static bool ValidarAltaCliente(string nombre, string tipoDocumento, string nroDocumento, string telefono, out string mensajeError)
        {
            return ValidarDatos(0, nombre, tipoDocumento, nroDocumento, telefono, out mensajeError);
        }

        public static bool ValidarEdicionCliente(int idClienteActual, string nombre, string tipoDocumento, string nroDocumento, string telefono, out string mensajeError)
        {
            return ValidarDatos(idClienteActual, nombre, tipoDocumento, nroDocumento, telefono, out mensajeError);
        }

        // Alta y edicion comparten las mismas reglas; idClienteActual = 0 en un alta
        private static bool ValidarDatos(int idClienteActual, string nombre, string tipoDocumento, string nroDocumento, string telefono, out string mensajeError)
        {
            string tipoDoc = NormalizarTipoDocumento(tipoDocumento);
            nroDocumento = (nroDocumento ?? string.Empty).Trim();
            telefono = (telefono ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(nombre))
            {
                mensajeError = "Por favor ingresá el Nombre y Apellido (o Razón Social) del cliente.";
                return false;
            }

            if (nombre.Trim().Length > 50)
            {
                mensajeError = "El nombre no puede superar los 50 caracteres.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(nroDocumento))
            {
                mensajeError = "El número de documento es obligatorio.";
                return false;
            }

            if (!RegexNumeros.IsMatch(nroDocumento))
            {
                mensajeError = "El número de documento debe contener solo números.";
                return false;
            }

            if (tipoDoc == "DNI" && (nroDocumento.Length < 7 || nroDocumento.Length > 8))
            {
                mensajeError = "El DNI debe contener 7 u 8 dígitos numéricos.";
                return false;
            }

            if ((tipoDoc == "CUIT" || tipoDoc == "CUIL") && nroDocumento.Length != 11)
            {
                mensajeError = "El " + tipoDoc + " debe contener exactamente 11 dígitos numéricos.";
                return false;
            }

            if (nroDocumento.Length > 20)
            {
                mensajeError = "El número de documento es demasiado largo.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(telefono))
            {
                mensajeError = "El teléfono de contacto es obligatorio.";
                return false;
            }

            if (!RegexNumeros.IsMatch(telefono) || telefono.Length < 8 || telefono.Length > 20)
            {
                mensajeError = "El teléfono debe contener solo números (entre 8 y 20 dígitos).";
                return false;
            }

            // La base tiene UNIQUE (tipo_documento, nro_documento): se avisa antes de llegar a ella
            if (DatosGlobales.Clientes.Any(c => c.Dni == nroDocumento
                                             && NormalizarTipoDocumento(c.TipoDocumento) == tipoDoc
                                             && c.IdCliente != idClienteActual))
            {
                mensajeError = "Ya existe otro cliente registrado con ese documento.";
                return false;
            }

            mensajeError = string.Empty;
            return true;
        }

        // ---------------------------------------------------------------
        // ALTA
        // ---------------------------------------------------------------

        // Arma el objeto en memoria. El Id real lo genera la base (IDENTITY) al guardar.
        public static ClienteItem CrearCliente(string nombre, string nroDocumento, string telefono, string tipoCliente, string tipoDocumento, bool activo)
        {
            return new ClienteItem
            {
                IdCliente = 0,
                TipoCliente = NormalizarTipoCliente(tipoCliente, tipoDocumento),
                NombreCompleto = nombre.Trim(),
                TipoDocumento = NormalizarTipoDocumento(tipoDocumento),
                Dni = nroDocumento.Trim(),
                Telefono = telefono.Trim(),
                Activo = activo
            };
        }

        // Guarda en la base, recibe el Id generado y lo agrega a la lista en memoria.
        // Puede lanzar InvalidOperationException (documento duplicado).
        public static void AgregarCliente(ClienteItem nuevoCliente)
        {
            if (nuevoCliente == null) return;

            int idGenerado = _datos.Insertar(
                nuevoCliente.TipoCliente,
                nuevoCliente.NombreCompleto,
                nuevoCliente.TipoDocumento,
                nuevoCliente.Dni,
                nuevoCliente.Telefono);

            nuevoCliente.IdCliente = idGenerado;

            // sp_cliente_insertar siempre crea el cliente activo; si se pidio inactivo, se da de baja
            if (!nuevoCliente.Activo)
                _datos.Baja(idGenerado);

            DatosGlobales.Clientes.Insert(0, nuevoCliente);
        }

        // ---------------------------------------------------------------
        // BAJA LOGICA / REACTIVAR
        // ---------------------------------------------------------------
        public static bool CambiarEstadoActivo(int idCliente, bool nuevoEstado)
        {
            var cliente = DatosGlobales.Clientes.FirstOrDefault(c => c.IdCliente == idCliente);
            if (cliente == null) return false;

            if (nuevoEstado)
                _datos.Reactivar(idCliente);
            else
                _datos.Baja(idCliente);

            cliente.Activo = nuevoEstado;
            return true;
        }

        // ---------------------------------------------------------------
        // MODIFICAR
        // ---------------------------------------------------------------
        public static void ModificarCliente(ClienteItem cliente, string nombre, string nroDocumento, string telefono, string tipoCliente, string tipoDocumento, bool activo)
        {
            if (cliente == null) return;

            string nuevoTipoDoc = string.IsNullOrWhiteSpace(tipoDocumento) ? cliente.TipoDocumento : NormalizarTipoDocumento(tipoDocumento);
            string nuevoTipoCliente = NormalizarTipoCliente(
                string.IsNullOrWhiteSpace(tipoCliente) ? cliente.TipoCliente : tipoCliente,
                nuevoTipoDoc);

            // Primero la base: si falla (ej. documento duplicado), la lista en memoria no se toca
            _datos.Modificar(
                cliente.IdCliente,
                nuevoTipoCliente,
                nombre.Trim(),
                nuevoTipoDoc,
                nroDocumento.Trim(),
                telefono.Trim());

            cliente.NombreCompleto = nombre.Trim();
            cliente.Dni = nroDocumento.Trim();
            cliente.Telefono = telefono.Trim();
            cliente.TipoCliente = nuevoTipoCliente;
            cliente.TipoDocumento = nuevoTipoDoc;

            // sp_cliente_modificar no toca "activo": si cambio, se usa baja/reactivar
            if (cliente.Activo != activo)
                CambiarEstadoActivo(cliente.IdCliente, activo);
        }

        // ---------------------------------------------------------------
        // AUXILIARES
        // ---------------------------------------------------------------

        // La base solo acepta 'PERSONA' o 'EMPRESA'. Textos como "Consumidor Final" o
        // "Responsable Inscripto" darian error, asi que se traducen: CUIT o "EMPRESA" => EMPRESA, el resto => PERSONA
        private static string NormalizarTipoCliente(string tipoCliente, string tipoDocumento)
        {
            bool esEmpresa = string.Equals(tipoCliente?.Trim(), "EMPRESA", StringComparison.OrdinalIgnoreCase)
                          || NormalizarTipoDocumento(tipoDocumento) == "CUIT";

            return esEmpresa ? "EMPRESA" : "PERSONA";
        }

        private static string NormalizarTipoDocumento(string tipoDocumento)
        {
            string t = (tipoDocumento ?? string.Empty).Trim().ToUpper();
            return TiposDocumentoValidos.Contains(t) ? t : "DNI";
        }

        private static ClienteItem ConvertirAItem(ClienteRow fila)
        {
            return new ClienteItem
            {
                IdCliente = fila.IdCliente,
                TipoCliente = fila.TipoCliente,
                NombreCompleto = fila.NombreRazonSocial,
                TipoDocumento = fila.TipoDocumento,
                Dni = fila.NroDocumento,
                Telefono = fila.Telefono,
                Activo = fila.Activo,
                CantidadCompras = fila.Compras
            };
        }
    }
}