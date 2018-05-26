using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ProyectoTransportesAndes.Models
{
    public class Usuario
    {
        [JsonProperty(PropertyName ="usuario")]
        public string Ususario { get; set; }
        [JsonProperty(PropertyName = "password")]
        public string Password { get; set; }
        [JsonProperty(PropertyName = "nombre")]
        public string Nombre { get; set; }
        [JsonProperty(PropertyName = "apellido")]
        public string Apellido { get; set; }
        [JsonProperty(PropertyName = "documento")]
        public string Documento { get; set; }
        [JsonProperty(PropertyName = "telefono")]
        public string Telefono { get; set; }
        [JsonProperty(PropertyName = "direccion")]
        public string Direccion { get; set; }
        [JsonProperty(PropertyName = "fNacimiento")]
        public string FNacimiento { get; set; }


    }
}
