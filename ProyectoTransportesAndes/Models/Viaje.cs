using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ProyectoTransportesAndes.Models
{
    public class Viaje
    {
        public Vehiculo Vehiculo { get; set; }
        public Cliente Cliente { get; set; }
        public List<Item> Items { get; set; }
        public DateTime Fecha { get; set; }
        public bool Realizado { get; set; }
        public double Calificacion { get; set; }
        public List<Peon> Peones { get; set; }
        public string Comentarios { get; set; }
    }
}
