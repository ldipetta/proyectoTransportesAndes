using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProyectoTransportesAndes.Models
{
    public class PosicionSatelital
    {
        #region Propiedades
        public string Id { get; set; }
        public string Latitud { get; set; }
        public string Longitud { get; set; }
        #endregion

        #region Constructores
        public PosicionSatelital(string id, string latitud, string longitud)
        {
            Id = id;
            Latitud = latitud;
            Longitud = longitud;
        }
        public PosicionSatelital()
        {
            Id = "";
            Latitud = "";
            Longitud = "";
        }
        #endregion
    }
}
