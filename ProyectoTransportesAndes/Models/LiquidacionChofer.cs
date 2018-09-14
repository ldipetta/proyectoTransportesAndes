using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProyectoTransportesAndes.Models
{
    public class LiquidacionChofer
    {

        [BsonId]
        public ObjectId Id { get; set; }
        public List<Viaje> Viajes { get; set; }
        public Chofer Chofer { get; set; }
        public double TotalViajes { get; set; }
        public double TotalComision { get; set; }
        public double Liquidacion { get; set; }
        public bool Pendiente { get; set; }
        public string Administrativo { get; set; }
        public DateTime FechaLiquidacion { get; set; }
       

        public LiquidacionChofer()
        {
            Viajes = new List<Viaje>();
            Chofer = new Chofer();
           
        }
    }
}
