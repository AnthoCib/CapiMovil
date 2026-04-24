using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace CapiMovil.PL.Gui.Models.ViewModels
{
    public class EstudianteFormViewModel
    {
        public Guid IdEstudiante { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un padre de familia.")]
        [Display(Name = "Padre de familia")]
        public Guid IdPadre { get; set; }

        [Display(Name = "Código")]
        public string CodigoEstudiante { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los nombres son obligatorios.")]
        [Display(Name = "Nombres")]
        public string Nombres { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido paterno es obligatorio.")]
        [Display(Name = "Apellido paterno")]
        public string ApellidoPaterno { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido materno es obligatorio.")]
        [Display(Name = "Apellido materno")]
        public string ApellidoMaterno { get; set; } = string.Empty;

        [StringLength(8, ErrorMessage = "El DNI debe tener 8 caracteres.")]
        [Display(Name = "DNI")]
        public string? DNI { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Fecha de nacimiento")]
        public DateTime? FechaNacimiento { get; set; }

        [Display(Name = "Género")]
        public string? Genero { get; set; }

        [Display(Name = "Grado")]
        public string? Grado { get; set; }

        [Display(Name = "Sección")]
        public string? Seccion { get; set; }

        [Display(Name = "Dirección")]
        public string? Direccion { get; set; }

        [Display(Name = "Latitud casa")]
        [Range(-90d, 90d, ErrorMessage = "La latitud debe estar entre -90 y 90.")]
        public decimal? LatitudCasa { get; set; }

        [Display(Name = "Longitud casa")]
        [Range(-180d, 180d, ErrorMessage = "La longitud debe estar entre -180 y 180.")]
        public decimal? LongitudCasa { get; set; }

        [Display(Name = "Foto")]
        public IFormFile? FotoArchivo { get; set; }

        public string? FotoUrl { get; set; }

        [Display(Name = "Observaciones")]
        public string? Observaciones { get; set; }

        [Display(Name = "Estado")]
        public bool Estado { get; set; } = true;

        public List<SelectListItem> Padres { get; set; } = new();

        public List<SelectListItem> Generos { get; set; } = new();
    }
}
