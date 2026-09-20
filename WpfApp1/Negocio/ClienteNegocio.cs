using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using TiendaOga.Entidades;

namespace TiendaOga.Negocio
{
    public static class ClienteNegocio
    {
        private static readonly Regex RegexNumeros = new Regex(@"^[0-9]+$");

        public static ObservableCollection<ClienteItem> ObtenerTodos()
        {
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

        public static void AgregarCliente(ClienteItem nuevoCliente)
        {
            if (nuevoCliente != null)
            {
                DatosGlobales.Clientes.Insert(0, nuevoCliente);
            }
        }

        public static bool CambiarEstadoActivo(int idCliente, bool nuevoEstado)
        {
            var cliente = DatosGlobales.Clientes.FirstOrDefault(c => c.IdCliente == idCliente);
            if (cliente == null) return false;

            cliente.Activo = nuevoEstado;
            return true;
        }

        public static void ModificarCliente(ClienteItem cliente, string nombre, string nroDocumento, string telefono, string tipoCliente, string tipoDocumento, bool activo)
        {
            if (cliente == null) return;

            cliente.NombreCompleto = nombre.Trim();
            cliente.Dni = nroDocumento.Trim();
            cliente.Telefono = telefono.Trim();
            cliente.TipoCliente = string.IsNullOrWhiteSpace(tipoCliente) ? cliente.TipoCliente : tipoCliente;
            cliente.TipoDocumento = string.IsNullOrWhiteSpace(tipoDocumento) ? cliente.TipoDocumento : tipoDocumento;
            cliente.Activo = activo;
        }
    }
}