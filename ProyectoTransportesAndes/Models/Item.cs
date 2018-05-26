using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ProyectoTransportesAndes.Models
{
    public class Item
    {
        [JsonProperty(PropertyName = "alto")]
        public double Alto { get; set; }
        [JsonProperty(PropertyName = "ancho")]
        public double Ancho { get; set; }
        [JsonProperty(PropertyName = "profundidad")]
        public double Profundidad { get; set; }
        [JsonProperty(PropertyName = "peso")]
        public double Peso { get; set; }
        [JsonProperty(PropertyName = "descripcion")]
        public string Descripcion { get; set; }
        [JsonProperty(PropertyName = "imagen")]
        public string Imagen { get; set; }
    }
}
