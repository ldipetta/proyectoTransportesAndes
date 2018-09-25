using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace ProyectoTransportesAndes.Models
{
    public class Peon
    {
        #region Propiedades
        [BsonId]
        public ObjectId Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Documento { get; set; }
        [DataType(DataType.PhoneNumber)]
        public string Telefono { get; set; }
        public string Direccion { get; set; }
        [Display(Name ="Fecha de nacimiento")]
        [DataType(DataType.Date)]
        public DateTime FNacimiento { get; set; }
        [DataType(DataType.ImageUrl)]
        public string Foto { get; set; }
        #endregion

        #region Metodos
        /// <summary>
        /// recibe un peon desencriptado y lo devuelve encriptado
        /// </summary>
        /// <param name="peon"></param>
        /// <returns>peon encriptado</returns>
        public Peon Encriptar(Peon peon)
        {
            peon.Apellido = Seguridad.Encriptar(peon.Apellido);
            peon.Direccion = Seguridad.Encriptar(peon.Direccion);
            peon.Documento = Seguridad.Encriptar(peon.Documento);
            peon.Nombre = Seguridad.Encriptar(peon.Nombre);
            peon.Telefono = Seguridad.Encriptar(peon.Telefono);
            return peon;
        }
        /// <summary>
        /// recibe un peon encriptado y lo devuelve desencriptado
        /// </summary>
        /// <param name="peon"></param>
        /// <returns>peon desencriptado</returns>
        public Peon Desencriptar(Peon peon)
        {
            peon.Apellido = Seguridad.Desencriptar(peon.Apellido);
            peon.Direccion = Seguridad.Desencriptar(peon.Direccion);
            peon.Documento = Seguridad.Desencriptar(peon.Documento);
            peon.Nombre = Seguridad.Desencriptar(peon.Nombre);
            peon.Telefono = Seguridad.Desencriptar(peon.Telefono);
            return peon;
        }
        #endregion
    }

}
