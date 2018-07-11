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
        [BsonId]
        public ObjectId Id { get; set; }
        [Display(Name ="Calificación")]
        public double Calificacion { get; set; }
        [Display(Name ="Matricula")]
        public string Matricula { get; set; }
        [Display(Name ="Marca")]
        public string Marca { get; set; }
        [Display(Name ="Modelo")]
        public string Modelo { get; set; }
        [Display(Name ="Paquetes")]
        public int Unidades { get; set; }
        [Display(Name ="Capacidad carga (Kg)")]
        public int CapacidadCargaKg { get; set; }
        [Display(Name ="Vencimiento permiso portuario")]
        [DataType(DataType.Date)]
        public DateTime VencimientoPermisoPortuario { get; set; }
        [Display(Name ="Tarifa")]
        [DisplayFormat(DataFormatString = "{0:d}")]
        [DataType(DataType.Currency)]
        public int Tarifa { get; set; }
        [Display(Name ="Vencimiento seguro")]
        [DataType(DataType.Date)]
        public DateTime VencimientoSeguro { get; set; }
        [Display(Name ="Empresa aseguradora")]
        public string EmpresaAseguradora { get; set; }
        [Display(Name ="Cantidad pasajeros")]
        public int CantidadPasajeros { get; set; }
        //estos items son las cosas que carga el vehiculo, por defecto debe estar vacio
        public List<Item> Items { get; set; }

    }
}
