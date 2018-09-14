using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace ProyectoTransportesAndes.Models
{

    public class Usuario
    {
        #region Atributos
        [BsonId]
        public ObjectId Id { get; set; }
        [Display(Name ="Usuario")]
        [MaxLength(15, ErrorMessage = "El usuario no puede superar los 15 caracteres")]
        [Required(ErrorMessage ="El usuario no puede ser vacío")]
        public string User { get; set; }
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "La contraseña no puede ser vacía")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres"), MaxLength(12, ErrorMessage = "La contrasena debe tener como maximo 12 caracteres")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{6,12}$", ErrorMessage = "Entre 6 y 12 caracteres, que incluya al menos un digito, una letra mayúscula, una letra minuscula y un signo de puntuación(.!?;:)")]
        public string Password { get; set; }
        [Required(ErrorMessage = "El nombre no puede ser vacío")]
        [MaxLength(30, ErrorMessage = "El nombre no puede superar los 30 caracteres")]
        public string Nombre { get; set; }
        [Required(ErrorMessage = "El apellido no puede ser vacío")]
        [MaxLength(30, ErrorMessage = "El apellido no puede superar los 30 caracteres")]
        public string Apellido { get; set; }
        [DataType(DataType.EmailAddress, ErrorMessage ="No es una dirección de e-mail válida")]
        [Required(ErrorMessage = "El e-mail no puede ser vacío")]
        public string Email { get; set; }
        [Required(ErrorMessage = "El documento no puede ser vacío")]
        [MaxLength(8, ErrorMessage = "Ingrese el documento con el formato 12345679")]
        [MinLength(8,ErrorMessage ="Ingrese el documento con el formato 12345679")]
        public string Documento { get; set; }
        [DataType(DataType.PhoneNumber)]
        [Required(ErrorMessage = "El teléfono no puede ser vacío")]
        public string Telefono { get; set; }
        [Required(ErrorMessage = "La dirección no puede ser vacía")]
        [MaxLength(100, ErrorMessage = "La dirección no puede superar los 100 caracteres")]
        public string Direccion { get; set; }
        [DataType(DataType.Date)]
        [Display(Name ="Fecha de nacimiento")]
        [Required(ErrorMessage = "La fecha de nacimiento no puede ser vacía")]
        public string FNacimiento { get; set; }
        public string Tipo { get; set; }
        public PosicionSatelital Ubicacion { get; set; }
        #endregion

        #region Metodos
        /// <summary>
        /// recibe un usuario desencriptado y lo devuelve encriptado
        /// </summary>
        /// <param name="usuario"></param>
        /// <returns>usuario encriptado</returns>
        public Usuario Encriptar(Usuario usuario)
        {
            usuario.Apellido = Seguridad.Encriptar(usuario.Apellido);
            usuario.Direccion = Seguridad.Encriptar(usuario.Direccion);
            usuario.Documento = Seguridad.Encriptar(usuario.Documento);
            usuario.Email = Seguridad.Encriptar(usuario.Email);
            usuario.FNacimiento = Seguridad.Encriptar(usuario.FNacimiento);
            usuario.Nombre = Seguridad.Encriptar(usuario.Nombre);
            usuario.Password = Seguridad.Encriptar(usuario.Password);
            usuario.Telefono = Seguridad.Encriptar(usuario.Telefono);
            usuario.Tipo = Seguridad.Encriptar(usuario.Tipo);
            usuario.User = Seguridad.Encriptar(usuario.User);
            usuario.Ubicacion.Latitud = Seguridad.Encriptar(usuario.Ubicacion.Latitud);
            usuario.Ubicacion.Longitud = Seguridad.Encriptar(usuario.Ubicacion.Longitud);
            return usuario;
        }
        /// <summary>
        /// recibe un usuario encriptado y lo devuelve desencriptado
        /// </summary>
        /// <param name="usuario"></param>
        /// <returns>usuario desencriptado</returns>
        public Usuario Desencriptar(Usuario usuario)
        {
            usuario.Apellido = Seguridad.Desencriptar(usuario.Apellido);
            usuario.Direccion = Seguridad.Desencriptar(usuario.Direccion);
            usuario.Documento = Seguridad.Desencriptar(usuario.Documento);
            usuario.Email = Seguridad.Desencriptar(usuario.Email);
            usuario.FNacimiento = Seguridad.Desencriptar(usuario.FNacimiento);
            usuario.Nombre = Seguridad.Desencriptar(usuario.Nombre);
            usuario.Password = Seguridad.Desencriptar(usuario.Password);
            usuario.Telefono = Seguridad.Desencriptar(usuario.Telefono);
            usuario.Tipo = Seguridad.Desencriptar(usuario.Tipo);
            usuario.User = Seguridad.Desencriptar(usuario.User);
            usuario.Ubicacion.Latitud = Seguridad.Desencriptar(usuario.Ubicacion.Latitud);
            usuario.Ubicacion.Longitud = Seguridad.Desencriptar(usuario.Ubicacion.Longitud);
            return usuario;
        }
        public Usuario DesencriptarSuperUsuario(Usuario usuario)
        {
            usuario.Password = Seguridad.Desencriptar(usuario.Password);
            usuario.User = Seguridad.Desencriptar(usuario.User); 
            return usuario;
        }
        #endregion





    }
}
