using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ProyectoTransportesAndes.Models
{
    public class Vehiculo
    {
        [JsonProperty(PropertyName ="id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "calificacion")]
        public double Calificacion { get; set; }
        [JsonProperty(PropertyName = "matricula")]
        public string Matricula { get; set; }
        [JsonProperty(PropertyName = "marca")]
        public string Marca { get; set; }
        [JsonProperty(PropertyName = "modelo")]
        public string Modelo { get; set; }
        [JsonProperty(PropertyName = "unidades")]
        public int Unidades { get; set; }
        [JsonProperty(PropertyName = "capacidadCargaKg")]
        public int CapacidadCargaKg { get; set; }
        [JsonProperty(PropertyName = "permisoPortuario")]
        public PermisoPortuario PermisoPortuario { get; set; }
        [JsonProperty(PropertyName = "tarifa")]
        public Tarifa Tarifa { get; set; }
        [JsonProperty(PropertyName = "seguro")]
        public Seguro Seguro { get; set; }
        [JsonProperty(PropertyName = "cantidadPasajeros")]
        public int CantidadPasajeros { get; set; }
        [JsonProperty(PropertyName = "items")]
        public List<Item> Items { get; set; }

    }
}
