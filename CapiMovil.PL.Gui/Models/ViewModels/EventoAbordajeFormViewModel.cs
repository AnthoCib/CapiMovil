using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace CapiMovil.PL.Gui.Models.ViewModels
{
    public class EventoAbordajeFormViewModel
    {
        public Guid IdEvento { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un recorrido.")]
        [Display(Name = "Recorrido")]
        public Guid IdRecorrido { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un estudiante.")]
        [Display(Name = "Estudiante")]
        public Guid IdEstudiante { get; set; }

        [Display(Name = "Paradero")]
        public Guid? IdParadero { get; set; }

        [Display(Name = "Registrado Por")]
        public Guid? RegistradoPor { get; set; }

        [Display(Name = "Código")]
        public string CodigoEvento { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe seleccionar el tipo de evento.")]
        [Display(Name = "Tipo Evento")]
        public string TipoEvento { get; set; } = "SUBIDA";

        [Required(ErrorMessage = "La fecha y hora son obligatorias.")]
        [Display(Name = "Fecha y Hora")]
        public DateTime FechaHora { get; set; } = DateTime.Now;

        [Display(Name = "Observación")]
        public string? Observacion { get; set; }

        [Display(Name = "Estado")]
        public bool Estado { get; set; } = true;

        public List<SelectListItem> Recorridos { get; set; } = new();
        public List<SelectListItem> Estudiantes { get; set; } = new();
        public List<SelectListItem> Paraderos { get; set; } = new();
        public List<SelectListItem> TiposEvento { get; set; } = new();
    }
}