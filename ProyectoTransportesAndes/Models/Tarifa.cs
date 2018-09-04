using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProyectoTransportesAndes.Models
{
    public class Tarifa
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public int Camioneta { get; set; }
        public int CamionChico { get; set; }
        public int Camion { get; set; }
        public int CamionGrande { get; set; }
        public int CamionMudanza { get; set; }
        public string UsuarioModificacion { get; set; }
        public DateTime FechaModificacion { get; set; }
    }
}
