using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProyectoTransportesAndes.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ProyectoTransportesAndes.ViewModels
{
    public class ViewModelViaje
    {
       
        public List<Vehiculo> Vehiculos { get; set; }
        //public List<Item> Items { get; set; }
        public Item Item { get; set; }
        public TipoItem TipoItem { get; set; }
        public SelectList ListaTipoItem { get; set; }
        public string LatitudOrigen { get; set; }
        public string LongitudOrigen { get; set; }
        public Viaje Viaje { get; set; }
        public string IdViaje { get; set; }
        public double PrecioEstimado { get; set; }
        //public TimeSpan HoraEstimadaLlegada { get; set; }
        public string HoraInicio { get; set; }
        public string HoraEstimadaLlegada { get; set; }

        public ViewModelViaje()
        {
           // Items = new List<Item>();
            cargarDatos();
        }
        private void cargarDatos()
        {
            var selectList = new List<SelectListItem>();

            var enumValues = Enum.GetValues(typeof(TipoItem)) as TipoItem[];

            foreach (var enumValue in enumValues)
            {
                selectList.Add(new SelectListItem
                {
                    Value = enumValue.ToString(),
                    Text = GetItemName(enumValue)
                });
            }
            ListaTipoItem = new SelectList(selectList,"Value","Value");
        }
        private string GetItemName(TipoItem value)
        {
            var itemInfo = value.GetType().GetMember(value.ToString());
            if (itemInfo.Length != 1)
                return null;

            var displayAttribute = itemInfo[0].GetCustomAttributes(typeof(DisplayAttribute), false)
                                   as DisplayAttribute[];
            if (displayAttribute == null || displayAttribute.Length != 1)
                return null;

            return displayAttribute[0].Name;
        }

    }
}
