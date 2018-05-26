using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ProyectoTransportesAndes.Models
{
    public class Camion:Vehiculo
    {
        [JsonProperty(PropertyName = "sucta")]
        public Sucta Sucta { get; set; }
        [JsonProperty(PropertyName = "mtop")]
        public Mtop Mtop { get; set; }
    }
}
