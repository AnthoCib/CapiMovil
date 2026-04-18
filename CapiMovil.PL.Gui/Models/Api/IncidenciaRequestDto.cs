using System.ComponentModel.DataAnnotations;

namespace CapiMovil.PL.Gui.Models.Api
{
    public class IncidenciaRequestDto
    {
        [Required]
        public Guid IdRecorrido { get; set; }

        public Guid? ReportadoPor { get; set; }

        [Required]
        [StringLength(50)]
        public string TipoIncidencia { get; set; } = string.Empty;

        [Required]
        [StringLength(300)]
        public string Descripcion { get; set; } = string.Empty;

        [Required]
        public DateTime FechaHora { get; set; }

        [Required]
        public string EstadoIncidencia { get; set; } = "PENDIENTE";

        [Required]
        public string Prioridad { get; set; } = "MEDIA";

        [StringLength(300)]
        public string? Solucion { get; set; }
    }
}