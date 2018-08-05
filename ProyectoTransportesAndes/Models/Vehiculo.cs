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

        [Display(Name ="Largo")]
        public double Largo { get; set; }
        public double Ancho { get; set; }
        public double Alto { get; set; }

        [Display(Name ="Capacidad carga (Kg)")]
        public double CapacidadCargaKg { get; set; }
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
        public Chofer Chofer { get; set; }
        public PosicionSatelital PosicionSatelital { get; set; }
        public double Unidades { get; set; }
        public bool Disponible { get; set; }

        public Vehiculo() { }
        public Vehiculo(string matricula)
        {
            Matricula = matricula;
        }
       
    }
}
