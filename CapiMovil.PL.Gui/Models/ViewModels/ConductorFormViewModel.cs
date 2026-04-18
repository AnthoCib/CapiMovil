using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace CapiMovil.PL.Gui.Models.ViewModels
{
    public class ConductorFormViewModel
    {
        public Guid IdConductor { get; set; }

        [Display(Name = "Usuario")]
        public Guid IdUsuario { get; set; }

        
        [Display(Name = "Código")]
        public string CodigoConductor { get; set; } = string.Empty;

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

        [Required(ErrorMessage = "La licencia es obligatoria.")]
        [Display(Name = "Licencia")]
        public string Licencia { get; set; } = string.Empty;

        [Display(Name = "Categoría de licencia")]
        public string? CategoriaLicencia { get; set; }

        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        [Display(Name = "Dirección")]
        public string? Direccion { get; set; }

        [Display(Name = "Estado")]
        public bool Estado { get; set; } = true;

        public List<SelectListItem> Usuarios { get; set; } = new();
    }
}