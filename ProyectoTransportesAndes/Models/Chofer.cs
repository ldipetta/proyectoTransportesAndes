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
        [Display(Name ="Carnet de salud")]
        [DataType(DataType.Date)]
        [Required(ErrorMessage ="El vencimiento del carnet de salud no puede ser vacío")]
        public DateTime VencimientoCarneDeSalud { get; set; }
        [Display(Name ="Libreta de conducir")]
        public LibretaDeConducir LibretaDeConducir { get; set; }
        [Display(Name ="Foto")]
        [DataType(DataType.ImageUrl)]
        //[Required(ErrorMessage ="Debe ingresar la foto del chofer")]
        public string Foto { get; set; }
        public bool Disponible { get; set; }
        public string Leyenda { get; set; }
     

        public Chofer() : base()
        {
            User = "";
            Password = "";
            Nombre = "";
            Apellido = "";
            Email = "";
            Documento = "";
            Telefono = "";
            Direccion = "";
            FNacimiento = "";
            Numero = "";
            LibretaDeConducir libreta = new LibretaDeConducir();
            libreta.Categoria = "";
            libreta.FVencimiento = "";
            LibretaDeConducir = libreta;
            Foto = "";
            Tipo = "Chofer";
            Leyenda = "";
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
            Leyenda = "";
        }

        #region Metodos
        /// <summary>
        /// recibe un chofer desencriptado y lo devuelve encriptado
        /// </summary>
        /// <param name="chofer"></param>
        /// <returns>chofer encriptado</returns>
        public Chofer Encriptar(Chofer chofer)
        {
            chofer.Apellido = Seguridad.Encriptar(chofer.Apellido);
            chofer.Direccion = Seguridad.Encriptar(chofer.Direccion);
            chofer.Documento = Seguridad.Encriptar(chofer.Documento);
            chofer.Email = Seguridad.Encriptar(chofer.Email);
            chofer.FNacimiento = Seguridad.Encriptar(chofer.FNacimiento);
            chofer.Nombre = Seguridad.Encriptar(chofer.Nombre);
            chofer.Password = Seguridad.Encriptar(chofer.Password);
            chofer.Telefono = Seguridad.Encriptar(chofer.Telefono);
            chofer.Tipo = Seguridad.Encriptar(chofer.Tipo);
            chofer.User = Seguridad.Encriptar(chofer.User);
            chofer.Ubicacion.Latitud = Seguridad.Encriptar(chofer.Ubicacion.Latitud);
            chofer.Ubicacion.Longitud = Seguridad.Encriptar(chofer.Ubicacion.Longitud);
            chofer.LibretaDeConducir.Categoria = Seguridad.Encriptar(chofer.LibretaDeConducir.Categoria);
            chofer.LibretaDeConducir.FVencimiento = Seguridad.Encriptar(chofer.LibretaDeConducir.FVencimiento);
            chofer.Leyenda = Seguridad.Encriptar(chofer.Leyenda);
            return chofer;
        }
        /// <summary>
        /// recibe un chofer encriptado y lo devuelve desencriptado
        /// </summary>
        /// <param name="chofer"></param>
        /// <returns>chofer desencriptado</returns>
        public Chofer Desencriptar(Chofer chofer)
        {
            chofer.Apellido = Seguridad.Desencriptar(chofer.Apellido);
            chofer.Direccion = Seguridad.Desencriptar(chofer.Direccion);
            chofer.Documento = Seguridad.Desencriptar(chofer.Documento);
            chofer.Email = Seguridad.Desencriptar(chofer.Email);
            chofer.FNacimiento = Seguridad.Desencriptar(chofer.FNacimiento);
            chofer.Nombre = Seguridad.Desencriptar(chofer.Nombre);
            chofer.Password = Seguridad.Desencriptar(chofer.Password);
            chofer.Telefono = Seguridad.Desencriptar(chofer.Telefono);
            chofer.Tipo = Seguridad.Desencriptar(chofer.Tipo);
            chofer.User = Seguridad.Desencriptar(chofer.User);
            chofer.Ubicacion.Latitud = Seguridad.Desencriptar(chofer.Ubicacion.Latitud);
            chofer.Ubicacion.Longitud = Seguridad.Desencriptar(chofer.Ubicacion.Longitud);
            chofer.LibretaDeConducir.Categoria = Seguridad.Desencriptar(chofer.LibretaDeConducir.Categoria);
            chofer.LibretaDeConducir.FVencimiento = Seguridad.Desencriptar(chofer.LibretaDeConducir.FVencimiento);
            chofer.Leyenda = Seguridad.Desencriptar(chofer.Leyenda);
            return chofer;
        }
        #endregion
    }
}
