using System.ComponentModel.DataAnnotations;

namespace CapiMovil.PL.Gui.Models.ViewModels
{
    public class CalificacionConductorViewModel
    {
        public Guid IdEstudiante { get; set; }
        public Guid IdConductor { get; set; }
        public string NombreConductor { get; set; } = string.Empty;
        public string NombreEstudiante { get; set; } = string.Empty;

        [Range(1, 5, ErrorMessage = "Seleccione una calificación entre 1 y 5.")]
        public int Puntaje { get; set; } = 5;

        [StringLength(250)]
        public string? Comentario { get; set; }
    }
}
