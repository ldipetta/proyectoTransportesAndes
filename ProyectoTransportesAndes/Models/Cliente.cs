using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace ProyectoTransportesAndes.Models
{
    public class Cliente:Usuario
    {
        #region Atributos
        public TarjetaDeCredito Tarjeta { get; set; }
        public string RazonSocial { get; set; }
        public string Rut { get; set; }
        public string Leyenda { get; set; }
        #endregion
        #region Constructores
        public Cliente(string usuario, string pass, string razonSocial,string rut, string nombre, string apellido, string email, string documento, string telefono, string direccion, string fNacimiento, string numeroTarjeta, string fVencTarjeta) : base()
        {
            User = usuario;
            Password = pass;
            Nombre = nombre;
            Apellido = apellido;
            Email = email;
            Documento = documento;
            Telefono = telefono;
            Direccion = direccion;
            FNacimiento = fNacimiento;
            TarjetaDeCredito tarjeta = new TarjetaDeCredito();
            tarjeta.fVencimiento = fVencTarjeta;
            tarjeta.Numero = numeroTarjeta;
            Tarjeta = tarjeta;
            Tipo = "Cliente";
            RazonSocial = razonSocial;
            Rut = rut;
            Leyenda = "";
        }
        public Cliente():base() { }
        #endregion
        #region Metodos
        /// <summary>
        /// recibe un cliente desencriptado y lo devuelve encriptado
        /// </summary>
        /// <param name="cliente"></param>
        /// <returns>cliente encriptado</returns>
        public Cliente Encriptar(Cliente cliente)
        {
            cliente.Apellido = Seguridad.Encriptar(cliente.Apellido);
            cliente.Direccion = Seguridad.Encriptar(cliente.Direccion);
            cliente.Documento = Seguridad.Encriptar(cliente.Documento);
            cliente.Email = Seguridad.Encriptar(cliente.Email);
            cliente.FNacimiento = Seguridad.Encriptar(cliente.FNacimiento);
            cliente.Leyenda = Seguridad.Encriptar(cliente.Leyenda);
            cliente.Nombre = Seguridad.Encriptar(cliente.Nombre);
            cliente.Password = Seguridad.Encriptar(cliente.Password);
            cliente.RazonSocial = Seguridad.Encriptar(cliente.RazonSocial);
            cliente.Rut = Seguridad.Encriptar(cliente.Rut);
            cliente.Tarjeta.fVencimiento = Seguridad.Encriptar(cliente.Tarjeta.fVencimiento);
            cliente.Tarjeta.Numero = Seguridad.Encriptar(cliente.Tarjeta.Numero);
            cliente.Telefono = Seguridad.Encriptar(cliente.Telefono);
            cliente.Tipo = Seguridad.Encriptar(cliente.Tipo);
            cliente.User = Seguridad.Encriptar(cliente.User);
            cliente.Ubicacion.Latitud = Seguridad.Encriptar(cliente.Ubicacion.Latitud);
            cliente.Ubicacion.Longitud = Seguridad.Encriptar(cliente.Ubicacion.Longitud);
            return cliente;
        }
        /// <summary>
        /// recibe un cliente encriptado y lo devuelve desencriptado
        /// </summary>
        /// <param name="cliente"></param>
        /// <returns>cliente desencriptado</returns>
        public Cliente Desencriptar(Cliente cliente)
        {
            cliente.Apellido = Seguridad.Desencriptar(cliente.Apellido);
            cliente.Direccion = Seguridad.Desencriptar(cliente.Direccion);
            cliente.Documento = Seguridad.Desencriptar(cliente.Documento);
            cliente.Email = Seguridad.Desencriptar(cliente.Email);
            cliente.FNacimiento = Seguridad.Desencriptar(cliente.FNacimiento);
            cliente.Leyenda = Seguridad.Desencriptar(cliente.Leyenda);
            cliente.Nombre = Seguridad.Desencriptar(cliente.Nombre);
            cliente.Password = Seguridad.Desencriptar(cliente.Password);
            cliente.RazonSocial = Seguridad.Desencriptar(cliente.RazonSocial);
            cliente.Rut = Seguridad.Desencriptar(cliente.Rut);
            cliente.Tarjeta.fVencimiento = Seguridad.Desencriptar(cliente.Tarjeta.fVencimiento);
            cliente.Tarjeta.Numero = Seguridad.Desencriptar(cliente.Tarjeta.Numero);
            cliente.Telefono = Seguridad.Desencriptar(cliente.Telefono);
            cliente.Tipo = Seguridad.Desencriptar(cliente.Tipo);
            cliente.User = Seguridad.Desencriptar(cliente.User);
            cliente.Ubicacion.Latitud = Seguridad.Desencriptar(cliente.Ubicacion.Latitud);
            cliente.Ubicacion.Longitud = Seguridad.Desencriptar(cliente.Ubicacion.Longitud);
            return cliente;
        }
        #endregion

    }
}
