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
        public TarjetaDeCredito Tarjeta { get; set; }
        public string RazonSocial { get; set; }
        public string Rut { get; set; }

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
        }
        public Cliente() { }

    }
}
