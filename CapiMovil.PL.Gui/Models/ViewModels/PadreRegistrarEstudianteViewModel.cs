using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace CapiMovil.PL.Gui.Models.ViewModels
{
    public class PadreRegistrarEstudianteViewModel
    {
        [Required(ErrorMessage = "Los nombres son obligatorios.")]
        [Display(Name = "Nombres")]
        public string Nombres { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido paterno es obligatorio.")]
        [Display(Name = "Apellido paterno")]
        public string ApellidoPaterno { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido materno es obligatorio.")]
        [Display(Name = "Apellido materno")]
        public string ApellidoMaterno { get; set; } = string.Empty;

        [Display(Name = "DNI")]
        [StringLength(8, MinimumLength = 8, ErrorMessage = "El DNI debe tener 8 dígitos.")]
        public string? DNI { get; set; }

        [Display(Name = "Fecha de nacimiento")]
        [DataType(DataType.Date)]
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
    }
}
