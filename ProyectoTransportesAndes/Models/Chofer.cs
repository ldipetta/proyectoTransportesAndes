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
        [Display(Name ="Vencimiento carnet de salud")]
        [DataType(DataType.Date)]
        [Required(ErrorMessage ="El vencimiento del carnet de salud no puede ser vacío")]
        public DateTime VencimientoCarneDeSalud { get; set; }
        [Display(Name ="Libreta de conducir")]
        public LibretaDeConducir LibretaDeConducir { get; set; }
        [Display(Name ="Foto")]
        [DataType(DataType.ImageUrl)]
        [Required(ErrorMessage ="Debe ingresar la foto del chofer")]
        public string Foto { get; set; }
        public bool Disponible { get; set; }
        public string Leyenda { get; set; }
     

        public Chofer() : base()
        {
        }
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
