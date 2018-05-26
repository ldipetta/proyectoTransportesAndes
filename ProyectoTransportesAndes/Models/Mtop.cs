using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ProyectoTransportesAndes.Models
{
    public class Mtop
    {
        [JsonProperty(PropertyName = "fVencimiento")]
        public DateTime FVencimiento { get; set; }
    }
}
