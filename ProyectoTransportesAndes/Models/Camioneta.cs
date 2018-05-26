using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ProyectoTransportesAndes.Models
{
    public class Camioneta:Vehiculo
    {
        [JsonProperty(PropertyName = "pasajeros")]
        public bool Pasajeros { get; set; }
    }
}
