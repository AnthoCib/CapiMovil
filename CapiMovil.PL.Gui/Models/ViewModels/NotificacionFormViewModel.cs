using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace CapiMovil.PL.Gui.Models
{
    public class NotificacionFormViewModel
    {
        public Guid IdNotificacion { get; set; }
        public string? CodigoNotificacion { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un padre de familia.")]
        [Display(Name = "Padre de familia")]
        public Guid IdPadre { get; set; }

        [Display(Name = "Estudiante")]
        public Guid? IdEstudiante { get; set; }

        [Required(ErrorMessage = "Debe ingresar el título.")]
        [StringLength(150)]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe ingresar el mensaje.")]
        [StringLength(300)]
        public string Mensaje { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe seleccionar el tipo.")]
        [Display(Name = "Tipo")]
        public string TipoNotificacion { get; set; } = "INFO";

        [Required(ErrorMessage = "Debe seleccionar el canal.")]
        public string Canal { get; set; } = "SISTEMA";

        public bool Leido { get; set; }

        public List<SelectListItem> Padres { get; set; } = new();
        public List<SelectListItem> Estudiantes { get; set; } = new();
        public List<SelectListItem> Tipos { get; set; } = new();
        public List<SelectListItem> Canales { get; set; } = new();
    }
}