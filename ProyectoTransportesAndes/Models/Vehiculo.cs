using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace ProyectoTransportesAndes.Models
{
    public class Vehiculo
    {
        //[JsonProperty(PropertyName ="id")]
        [BsonId]
        public ObjectId Id { get; set; }
        //[JsonProperty(PropertyName ="numero")]
        public string Numero { get; set; }
        //[JsonProperty(PropertyName = "calificacion")]
        [Display(Name ="Calificación")]
        public double Calificacion { get; set; }
        //[JsonProperty(PropertyName = "matricula")]
        [Display(Name ="Matricula")]
        public string Matricula { get; set; }
        //[JsonProperty(PropertyName = "marca")]
        [Display(Name ="Marca")]
        public string Marca { get; set; }
        //[JsonProperty(PropertyName = "modelo")]
        [Display(Name ="Modelo")]
        public string Modelo { get; set; }
        //[JsonProperty(PropertyName = "unidades")]
        [Display(Name ="Paquetes")]
        public int Unidades { get; set; }
        //[JsonProperty(PropertyName = "capacidadCargaKg")]
        [Display(Name ="Capacidad carga (Kg)")]
        public int CapacidadCargaKg { get; set; }
        //[JsonProperty(PropertyName = "vencimientoPermisoPortuario")]
        [Display(Name ="Permiso portuario")]
        public DateTime VencimientoPermisoPortuario { get; set; }
        //[JsonProperty(PropertyName = "tarifa")]
        [Display(Name ="Tarifa")]
        public int Tarifa { get; set; }
        //[JsonProperty(PropertyName = "vencimientoSeguro")]
        [Display(Name ="Vencimiento seguro")]
        public DateTime VencimientoSeguro { get; set; }
        //[JsonProperty(PropertyName ="empresaAseguradora")]
        [Display(Name ="Empresa aseguradora")]
        public string EmpresaAseguradora { get; set; }
        //[JsonProperty(PropertyName = "cantidadPasajeros")]
        [Display(Name ="Cantidad pasajeros")]
        public int CantidadPasajeros { get; set; }
        //[JsonProperty(PropertyName = "items")]
        public List<Item> Items { get; set; }

    }
}
