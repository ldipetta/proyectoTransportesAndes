﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace ProyectoTransportesAndes.Models
{
    public class Vehiculo
    {
        #region Propiedades
        [BsonId]
        public ObjectId Id { get; set; }
        [Display(Name ="Calificación")]
        public double Calificacion { get; set; }
        [Display(Name ="Matricula")]
        [Required(ErrorMessage ="Debe ingresar la matricula")]
        public string Matricula { get; set; }
        [Required(ErrorMessage ="Debe ingresar la marca")]
        [Display(Name ="Marca")]
        public string Marca { get; set; }
        [Display(Name ="Modelo")]
        [Required(ErrorMessage ="Debe ingresar el modelo")]
        public string Modelo { get; set; }
        [Required(ErrorMessage ="Debe ingresar el largo")]
        [Display(Name ="Largo(cm)")]
        public double Largo { get; set; }
        [Required(ErrorMessage ="Debe ingresar el ancho")]
        [Display(Name = "Ancho(cm)")]
        public double Ancho { get; set; }
        [Required(ErrorMessage ="Debe ingresar el alto")]
        [Display(Name = "Alto(cm)")]
        public double Alto { get; set; }
        [Display(Name ="Capacidad carga (Kg)")]
        [Required(ErrorMessage ="Debe ingresar la capacidad de carga")]
        public double CapacidadCargaKg { get; set; }
        [Display(Name ="Vencimiento permiso portuario")]
        [DataType(DataType.Date)]
        public DateTime VencimientoPermisoPortuario { get; set; }
        [Display(Name ="Tarifa")]
        [DisplayFormat(DataFormatString = "{0:d}")]
        [DataType(DataType.Currency)]
        [Required(ErrorMessage = "Debe ingresar la tarifa")]
        public int Tarifa { get; set; }
        [Display(Name ="Vencimiento seguro")]
        [DataType(DataType.Date)]
        [Required(ErrorMessage ="Debe ingresar el vencimiento del seguro")]
        public DateTime VencimientoSeguro { get; set; }
        [Display(Name ="Empresa aseguradora")]
        [Required(ErrorMessage ="Debe ingresar la empresa aseguradora")]
        public string EmpresaAseguradora { get; set; }
        [Display(Name ="Cantidad pasajeros")]
        [Required(ErrorMessage ="Debe ingresar la cantidad de pasajeros")]
        public int CantidadPasajeros { get; set; }
        public List<Item> Items { get; set; }
        public Chofer Chofer { get; set; }
        public PosicionSatelital PosicionSatelital { get; set; }
        public double Unidades { get; set; }
        public bool Disponible { get; set; }
        public TipoVehiculo Tipo { get; set; }
        public double ConsumoKml { get; set; }
        #endregion

        #region Constructores

        public Vehiculo()
        {
            Items = new List<Item>();
            Calificacion = 0;
            Id = new ObjectId();
            Matricula = "";
            Marca = "";
            Modelo = "";
            Unidades = 0;
            Tipo = TipoVehiculo.Otros;
            Disponible = true;
            PosicionSatelital = new PosicionSatelital();
            Chofer = new Chofer();
            CantidadPasajeros = 0;
            EmpresaAseguradora = "";
            VencimientoSeguro = DateTime.Now;
            Tarifa = 0;
            VencimientoPermisoPortuario = DateTime.Now;
            Alto = 0;
            Largo = 0;
            Ancho = 0;
            CantidadPasajeros = 0;
            EmpresaAseguradora = "";
            CapacidadCargaKg = 0;
            ConsumoKml = 0;
        }
        public Vehiculo(string matricula)
        {
            Matricula = matricula;
            Items = new List<Item>();
        }
        #endregion

        #region Metodos
        public override string ToString()
        {
            return "Matricula: "+ Matricula + "/Vehiculo: " + Marca + " " + Modelo;
        }
        #endregion

    }
}
