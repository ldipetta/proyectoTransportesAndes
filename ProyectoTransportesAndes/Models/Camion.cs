using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ProyectoTransportesAndes.Models
{
    public class Camion:Vehiculo
    {
        [JsonProperty(PropertyName = "vencimientoSucta")]
        public DateTime VencimientoSucta { get; set; }
        [JsonProperty(PropertyName = "vencimientoMtop")]
        public DateTime VecimientoMtop { get; set; }
    }
}
