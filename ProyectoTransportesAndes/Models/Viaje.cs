using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ProyectoTransportesAndes.Models
{
    public class Viaje
    {
        [JsonProperty(PropertyName = "vehiculo")]
        public Vehiculo Vehiculo { get; set; }
        [JsonProperty(PropertyName = "cliente")]
        public Cliente Cliente { get; set; }
        [JsonProperty(PropertyName = "items")]
        public List<Item> Items { get; set; }
        [JsonProperty(PropertyName = "fecha")]
        public DateTime Fecha { get; set; }
        [JsonProperty(PropertyName = "realizado")]
        public bool Realizado { get; set; }
        [JsonProperty(PropertyName = "calificacion")]
        public double Calificacion { get; set; }
        [JsonProperty(PropertyName = "peones")]
        public List<Peon> Peones { get; set; }
        [JsonProperty(PropertyName = "comentarios")]
        public string Comentarios { get; set; }
    }
}
