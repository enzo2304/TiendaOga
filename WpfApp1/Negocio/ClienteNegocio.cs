using System;
using System.Linq;
using System.Text.RegularExpressions;
using TiendaOga.Entidades;

namespace TiendaOga.Negocio
{
    public static class ClienteNegocio
    {
        private static readonly Regex RegexNumeros = new Regex(@"^[0-9]+$");

        public static bool ValidarAltaCliente(string nombre, string nroDocumento, string telefono, out string mensajeError)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                mensajeError = "Por favor ingresá el Nombre y Apellido del cliente.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(nroDocumento))
            {
                mensajeError = "El número de documento es obligatorio.";
                return false;
            }

            if (!RegexNumeros.IsMatch(nroDocumento) || nroDocumento.Length < 7 || nroDocumento.Length > 8)
            {
                mensajeError = "El DNI debe ser numérico y contener entre 7 y 8 dígitos.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(telefono))
            {
                mensajeError = "El teléfono de contacto es obligatorio.";
                return false;
            }

            if (!RegexNumeros.IsMatch(telefono) || telefono.Length < 8)
            {
                mensajeError = "El teléfono debe contener solo números (mínimo 8 dígitos).";
                return false;
            }

            if (DatosGlobales.Clientes.Any(c => c.Dni == nroDocumento))
            {
                mensajeError = "Ya existe un cliente registrado con ese número de documento.";
                return false;
            }

            mensajeError = string.Empty;
            return true;
        }

        // Misma validación que el alta, pero excluye al propio cliente del chequeo de DNI duplicado
        public static bool ValidarEdicionCliente(int idClienteActual, string nombre, string nroDocumento, string telefono, out string mensajeError)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                mensajeError = "Por favor ingresá el Nombre y Apellido del cliente.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(nroDocumento))
            {
                mensajeError = "El número de documento es obligatorio.";
                return false;
            }

            if (!RegexNumeros.IsMatch(nroDocumento) || nroDocumento.Length < 7 || nroDocumento.Length > 8)
            {
                mensajeError = "El DNI debe ser numérico y contener entre 7 y 8 dígitos.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(telefono))
            {
                mensajeError = "El teléfono de contacto es obligatorio.";
                return false;
            }

            if (!RegexNumeros.IsMatch(telefono) || telefono.Length < 8)
            {
                mensajeError = "El teléfono debe contener solo números (mínimo 8 dígitos).";
                return false;
            }

            if (DatosGlobales.Clientes.Any(c => c.Dni == nroDocumento && c.IdCliente != idClienteActual))
            {
                mensajeError = "Ya existe otro cliente registrado con ese número de documento.";
                return false;
            }

            mensajeError = string.Empty;
            return true;
        }

        public static ClienteItem CrearCliente(string nombre, string nroDocumento, string telefono, string tipoCliente, string tipoDocumento, bool activo)
        {
            return new ClienteItem
            {
                IdCliente = DatosGlobales.Clientes.Count == 0
                    ? 1
                    : DatosGlobales.Clientes.Max(c => c.IdCliente) + 1,
                TipoCliente = string.IsNullOrWhiteSpace(tipoCliente) ? "Consumidor Final" : tipoCliente,
                NombreCompleto = nombre.Trim(),
                TipoDocumento = string.IsNullOrWhiteSpace(tipoDocumento) ? "DNI" : tipoDocumento,
                Dni = nroDocumento.Trim(),
                Telefono = telefono.Trim(),
                Activo = activo
            };
        }
    }
}