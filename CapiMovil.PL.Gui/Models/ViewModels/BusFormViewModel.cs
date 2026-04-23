using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace CapiMovil.PL.Gui.Models.ViewModels
{
    public class BusFormViewModel
    {
        public Guid IdBus { get; set; }

        [Display(Name = "Código")]
        public string CodigoBus { get; set; } = string.Empty;

        [Required(ErrorMessage = "La placa es obligatoria.")]
        [Display(Name = "Placa")]
        public string Placa { get; set; } = string.Empty;

        [Required(ErrorMessage = "La marca es obligatoria.")]
        [Display(Name = "Marca")]
        public string? Marca { get; set; }

        [Display(Name = "Modelo")]
        public string? Modelo { get; set; }

        [Display(Name = "Color")]
        public string? Color { get; set; }

        [Display(Name = "Año")]
        [Range(1900, 2100, ErrorMessage = "Ingrese un año válido.")]
        public int? Anio { get; set; }

        [Required(ErrorMessage = "La capacidad es obligatoria.")]
        [Range(1, 200, ErrorMessage = "La capacidad debe ser mayor a 0.")]
        public int Capacidad { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un estado operativo.")]
        [Display(Name = "Estado Operativo")]
        public string EstadoOperacion { get; set; } = "ACTIVO";

        [Display(Name = "Seguro Vigente")]
        public bool SeguroVigente { get; set; } = true;

        [Display(Name = "Fecha Vencimiento SOAT")]
        [DataType(DataType.Date)]
        public DateTime? FechaVencimientoSOAT { get; set; }

        [Display(Name = "Fecha Revisión Técnica")]
        [DataType(DataType.Date)]
        public DateTime? FechaRevisionTecnica { get; set; }

        [Display(Name = "Estado")]
        public bool Estado { get; set; } = true;

        public List<SelectListItem> EstadosOperacion { get; set; } = new();
    }
}