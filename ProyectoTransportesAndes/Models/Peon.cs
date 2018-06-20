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
    }

}
