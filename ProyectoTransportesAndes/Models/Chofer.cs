using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ProyectoTransportesAndes.Models
{
    public class Chofer:Usuario
    {
        [JsonProperty(PropertyName = "carneDeSalud")]
        public CarneDeSalud CarneDeSalud { get; set; }
        [JsonProperty(PropertyName = "libretaDeConducir")]
        public LibretaDeConducir LibretaDeConducir { get; set; }
        [JsonProperty(PropertyName = "foto")]
        public string Foto { get; set; }
    }
}
