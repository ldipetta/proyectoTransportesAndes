using ProyectoTransportesAndes.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProyectoTransportesAndes.ViewModels
{
    public class ViewModelCliente
    {
        public Cliente Cliente { get; set; }
        public TarjetaDeCredito Tarjeta { get; set; }
        public string Id { get; set; }
        public string ConfirmarPassword { get; set; }

        public ViewModelCliente() { }
    }
}
