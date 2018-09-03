﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProyectoTransportesAndes.Models;

namespace ProyectoTransportesAndes.ViewModels
{
    public class ViewModelChofer
    {
        public Chofer Chofer { get; set; }
        public LibretaDeConducir Libreta { get; set; }
        public string Id { get; set; }
       
        public ViewModelChofer() { }
       
    }
}
