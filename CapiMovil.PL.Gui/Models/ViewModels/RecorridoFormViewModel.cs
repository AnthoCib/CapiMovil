using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace CapiMovil.PL.Gui.Models.ViewModels
{
    public class RecorridoFormViewModel
    {
        public Guid IdRecorrido { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una ruta.")]
        [Display(Name = "Ruta")]
        public Guid IdRuta { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un bus.")]
        [Display(Name = "Bus")]
        public Guid IdBus { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un conductor.")]
        [Display(Name = "Conductor")]
        public Guid IdConductor { get; set; }

        [Display(Name = "Código")]
        public string CodigoRecorrido { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha es obligatoria.")]
        [DataType(DataType.Date)]
        public DateTime Fecha { get; set; } = DateTime.Today;

        [Display(Name = "Hora Inicio Programada")]
        [DataType(DataType.Time)]
        public TimeSpan? HoraInicioProgramada { get; set; }

        [Display(Name = "Hora Fin Programada")]
        [DataType(DataType.Time)]
        public TimeSpan? HoraFinProgramada { get; set; }

        [Display(Name = "Estado Recorrido")]
        public string EstadoRecorrido { get; set; } = "PROGRAMADO";

        [Display(Name = "Observaciones")]
        public string? Observaciones { get; set; }

        [Display(Name = "Estado")]
        public bool Estado { get; set; } = true;

        public List<SelectListItem> Rutas { get; set; } = new();
        public List<SelectListItem> Buses { get; set; } = new();
        public List<SelectListItem> Conductores { get; set; } = new();
        public List<SelectListItem> EstadosRecorrido { get; set; } = new();
    }
}