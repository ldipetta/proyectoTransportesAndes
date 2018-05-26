using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ProyectoTransportesAndes.Models
{
    public class LibretaDeConducir
    {
        [JsonProperty(PropertyName = "categoria")]
        public string Categoria { get; set; }
        [JsonProperty(PropertyName = "fVencimiento")]
        public DateTime FVencimiento { get; set; }
    }
}
