using System.ComponentModel.DataAnnotations;

namespace CapiMovil.PL.Gui.Models.Api
{
    public class NotificacionRequestDto
    {
        [Required]
        public Guid IdPadre { get; set; }

        public Guid? IdEstudiante { get; set; }

        [Required]
        [StringLength(150)]
        public string Titulo { get; set; } = string.Empty;

        [Required]
        [StringLength(300)]
        public string Mensaje { get; set; } = string.Empty;

        [Required]
        public string TipoNotificacion { get; set; } = "INFO";

        [Required]
        public string Canal { get; set; } = "SISTEMA";

        public bool Leido { get; set; }
    }
}