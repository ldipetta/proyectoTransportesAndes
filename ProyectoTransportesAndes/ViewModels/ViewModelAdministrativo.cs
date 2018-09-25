using ProyectoTransportesAndes.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProyectoTransportesAndes.ViewModels
{
    public class ViewModelAdministrativo
    {
        public Administrativo Administrativo { get; set; }
        public bool Administrador { get; set; }
        public string Id { get; set; }
        public string ConfirmarPassword { get; set; }
    }
}
