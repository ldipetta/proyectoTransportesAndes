using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace ProyectoTransportesAndes.Models
{
    public class Chofer:Usuario
    {
        public string Numero { get; set; }
        [Display(Name ="Vencimiento carne de salud")]
        [DataType(DataType.Date)]
        public DateTime VencimientoCarneDeSalud { get; set; }
        public LibretaDeConducir LibretaDeConducir { get; set; }
        [Display(Name ="Foto")]
        [DataType(DataType.ImageUrl)]
        public string Foto { get; set; }

        public Chofer() : base() { }
        public Chofer(string usuario, string pass, string nombre, string apellido, string email, string documento, string telefono, string direccion, string fNacimiento, string numero, string vtoCarneSalud, string categoriaLibreta, string fVtoLibreta, string foto) : base()
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
            Numero = numero;
            LibretaDeConducir libreta = new LibretaDeConducir();
            libreta.Categoria = categoriaLibreta;
            libreta.FVencimiento = fVtoLibreta;
            LibretaDeConducir = libreta;
            Foto = foto;
            Tipo = "Chofer";
        }
    }
}
