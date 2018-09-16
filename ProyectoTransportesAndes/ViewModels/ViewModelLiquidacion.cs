using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using ProyectoTransportesAndes.Configuracion;
using ProyectoTransportesAndes.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProyectoTransportesAndes.ViewModels
{
    public class ViewModelLiquidacion
    {
        #region Propiedades
        private IOptions<AppSettingsMongo> _settings;
        public SelectList ListaChoferes { get; set; }
        public string IdChofer { get; set; }
        public LiquidacionChofer Liquidacion { get; set; }
        public string IdLiquidacionChofer { get; set; }
        public string Documento { get; set; }
        public bool Liquidar { get; set; }
        #endregion

        #region Constructores
        public ViewModelLiquidacion(IOptions<AppSettingsMongo> settings)
        {
            _settings = settings;
            cargarChoferes().Wait();

        }
        public ViewModelLiquidacion()
        {
            cargarChoferes().Wait();
        }
        #endregion

        #region Metodos
        public async Task cargarChoferes()
        {
            var clientes = await ControladoraUsuarios.getInstance(_settings).getChoferes();
            List<Chofer> lista = clientes.ToList();
            Chofer c = new Chofer() { Id = new ObjectId(), Leyenda = "Seleccione un chofer" };
            lista.Insert(0, c);
            ListaChoferes = new SelectList(lista, "Id", "Leyenda");
        }
        #endregion
    }
}
