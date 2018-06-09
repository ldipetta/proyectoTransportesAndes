using ProyectoTransportesAndes.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProyectoTransportesAndes.ViewModels
{
    public class ViewModelCliente
    {
        public Usuario Usuario { get; set; }
        public TarjetaDeCredito Tarjeta { get; set; }
    }
}
