using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ProyectoTransportesAndes.Models
{
    public class Sucta
    {
        [JsonProperty(PropertyName = "fVencimiento")]
        public DateTime Fvencimiento { get; set; }
    }
}
