using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace CapiMovil.PL.Gui.Models.ViewModels
{
    public class RutaFormViewModel
    {
        public Guid IdRuta { get; set; }

        [Display(Name = "Código")]
        public string CodigoRuta { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre de la ruta es obligatorio.")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "Debe seleccionar el turno.")]
        [Display(Name = "Turno")]
        public string Turno { get; set; } = "MANANA";

        [Required(ErrorMessage = "La hora de inicio es obligatoria.")]
        [Display(Name = "Hora Inicio")]
        [DataType(DataType.Time)]
        public TimeSpan HoraInicio { get; set; }

        [Required(ErrorMessage = "La hora de fin es obligatoria.")]
        [Display(Name = "Hora Fin")]
        [DataType(DataType.Time)]
        public TimeSpan HoraFin { get; set; }

        [Display(Name = "Punto Inicio")]
        public string? PuntoInicio { get; set; }

        [Display(Name = "Punto Fin")]
        public string? PuntoFin { get; set; }

        [Required(ErrorMessage = "Debe seleccionar el estado de la ruta.")]
        [Display(Name = "Estado Ruta")]
        public string EstadoRuta { get; set; } = "ACTIVA";

        [Display(Name = "Estado")]
        public bool Estado { get; set; } = true;

        public List<SelectListItem> Turnos { get; set; } = new();
        public List<SelectListItem> EstadosRuta { get; set; } = new();
    }
}