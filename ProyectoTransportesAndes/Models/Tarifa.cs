using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ProyectoTransportesAndes.Models
{
    public class Tarifa
    {
        [JsonProperty(PropertyName = "monto")]
        public int Monto { get; set; }
        [JsonProperty(PropertyName = "fecha")]
        public DateTime Fecha { get; set; }
    }
}
