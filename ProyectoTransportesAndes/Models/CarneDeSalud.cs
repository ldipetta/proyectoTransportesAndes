using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ProyectoTransportesAndes.Models
{
    public class CarneDeSalud
    {
        [JsonProperty(PropertyName = "fVencimiento")]
        public DateTime FVencimiento { get; set; }
        [JsonProperty(PropertyName = "emisario")]
        public string Emisario { get; set; }
        [JsonProperty(PropertyName = "observaciones")]
        public string Observaciones { get; set; }
    }
}
