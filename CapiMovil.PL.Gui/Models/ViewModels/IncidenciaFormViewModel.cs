using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace CapiMovil.PL.Gui.Models
{
    public class IncidenciaFormViewModel
    {
        public Guid IdIncidencia { get; set; }

        [Display(Name = "Código")]
        public string? CodigoIncidencia { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un recorrido.")]
        [Display(Name = "Recorrido")]
        public Guid IdRecorrido { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un conductor.")]
        [Display(Name = "Conductor")]
        public Guid IdConductor { get; set; }

        public Guid? ReportadoPor { get; set; }

        [Required(ErrorMessage = "Debe seleccionar el tipo de incidencia.")]
        [StringLength(50)]
        [Display(Name = "Tipo de incidencia")]
        public string TipoIncidencia { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe ingresar la descripción.")]
        [StringLength(300)]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe ingresar la fecha y hora.")]
        [Display(Name = "Fecha y hora")]
        public DateTime FechaHora { get; set; }

        [Required(ErrorMessage = "Debe seleccionar el estado.")]
        [Display(Name = "Estado")]
        public string EstadoIncidencia { get; set; } = "PENDIENTE";

        [Required(ErrorMessage = "Debe seleccionar la prioridad.")]
        [Display(Name = "Prioridad")]
        public string Prioridad { get; set; } = "MEDIA";

        [StringLength(300)]
        [Display(Name = "Solución")]
        public string? Solucion { get; set; }

        public List<SelectListItem> Recorridos { get; set; } = new();
        public List<SelectListItem> Conductores { get; set; } = new();
        public List<SelectListItem> TiposIncidencia { get; set; } = new();
        public List<SelectListItem> EstadosIncidencia { get; set; } = new();
        public List<SelectListItem> Prioridades { get; set; } = new();
    }
}