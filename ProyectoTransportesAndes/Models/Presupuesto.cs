using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProyectoTransportesAndes.Models
{
    public class Presupuesto
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public string DireccionOrigen { get; set; }
        public string DireccionDestino { get; set; }
        public Cliente Cliente { get; set; }
        public string Observaciones { get; set; }
        public bool Realizado { get; set; }

        public Presupuesto()
        {
            FechaSolicitud = DateTime.Now;
            DireccionOrigen = "";
            DireccionDestino = "";
            Cliente = new Cliente();
            Observaciones = "";
            Realizado = false;
        }
    }
}
