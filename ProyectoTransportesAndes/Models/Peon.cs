using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ProyectoTransportesAndes.Models
{
    public class Peon
    {
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
        public DateTime FNacimiento { get; set; }
        [JsonProperty(PropertyName = "foto")]
        public string Foto { get; set; }
    }
}
