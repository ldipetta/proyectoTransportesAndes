using Microsoft.AspNetCore.Mvc.Rendering;
using ProyectoTransportesAndes.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProyectoTransportesAndes.ViewModels
{
    public class ViewModelViaje
    {
        public SelectList ListaVehiculos { get; set; }
        public int VehiculoSeleccionado { get; set; }
        public SelectList Cliente { get; set; }
        public int ClienteSeleccionado { get; set; }
        public DateTime Fecha { get; set; }
        public double Calificacion { get; set; }

    }
}
