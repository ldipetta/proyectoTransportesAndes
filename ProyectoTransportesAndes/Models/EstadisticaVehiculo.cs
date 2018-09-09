using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProyectoTransportesAndes.Models
{
    public class EstadisticaVehiculo
    {
        public TimeSpan Mes { get; set; }
        public TimeSpan Año { get; set; }
        public double KmRecorridos { get; set; }
        public int CantViajesDirectos { get; set; }
        public double CantidadCombustible { get; set; }
        public double TotalRecaudado { get; set; }
        public int CantViajesOnline { get; set; }

    }
}
