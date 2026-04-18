using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace CapiMovil.PL.Gui.Models.ViewModels
{
    public class RutaEstudianteFormViewModel
    {
        public Guid IdRutaEstudiante { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una ruta.")]
        [Display(Name = "Ruta")]
        public Guid IdRuta { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un estudiante.")]
        [Display(Name = "Estudiante")]
        public Guid IdEstudiante { get; set; }

        [Display(Name = "Paradero de Subida")]
        public Guid? IdParaderoSubida { get; set; }

        [Display(Name = "Paradero de Bajada")]
        public Guid? IdParaderoBajada { get; set; }

        [Display(Name = "Código")]
        public string CodigoRutaEstudiante { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
        [Display(Name = "Inicio de Vigencia")]
        [DataType(DataType.Date)]
        public DateTime FechaInicioVigencia { get; set; } = DateTime.Today;

        [Display(Name = "Fin de Vigencia")]
        [DataType(DataType.Date)]
        public DateTime? FechaFinVigencia { get; set; }

        [Required(ErrorMessage = "Debe seleccionar el estado de asignación.")]
        [Display(Name = "Estado Asignación")]
        public string EstadoAsignacion { get; set; } = "ACTIVO";

        [Display(Name = "Observaciones")]
        public string? Observaciones { get; set; }

        [Display(Name = "Estado")]
        public bool Estado { get; set; } = true;

        public List<SelectListItem> Rutas { get; set; } = new();
        public List<SelectListItem> Estudiantes { get; set; } = new();
        public List<SelectListItem> ParaderosSubida { get; set; } = new();
        public List<SelectListItem> ParaderosBajada { get; set; } = new();
        public List<SelectListItem> EstadosAsignacion { get; set; } = new();
    }
}