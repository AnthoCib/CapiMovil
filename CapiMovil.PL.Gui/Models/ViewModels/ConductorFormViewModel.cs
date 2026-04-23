using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace CapiMovil.PL.Gui.Models.ViewModels
{
    public class ConductorFormViewModel
    {
        public Guid IdConductor { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un usuario.")]
        [Display(Name = "Usuario")]
        public Guid IdUsuario { get; set; }

        [Display(Name = "Código")]
        public string CodigoConductor { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los nombres son obligatorios.")]
        [StringLength(80, ErrorMessage = "Los nombres no deben superar los 80 caracteres.")]
        [Display(Name = "Nombres")]
        public string Nombres { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido paterno es obligatorio.")]
        [StringLength(60, ErrorMessage = "El apellido paterno no debe superar los 60 caracteres.")]
        [Display(Name = "Apellido paterno")]
        public string ApellidoPaterno { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido materno es obligatorio.")]
        [StringLength(60, ErrorMessage = "El apellido materno no debe superar los 60 caracteres.")]
        [Display(Name = "Apellido materno")]
        public string ApellidoMaterno { get; set; } = string.Empty;

        [RegularExpression("^[0-9]{8}$", ErrorMessage = "El DNI debe tener exactamente 8 dígitos.")]
        [Display(Name = "DNI")]
        public string? DNI { get; set; }

        [Required(ErrorMessage = "La licencia es obligatoria.")]
        [StringLength(30, ErrorMessage = "La licencia no debe superar los 30 caracteres.")]
        [Display(Name = "Licencia")]
        public string Licencia { get; set; } = string.Empty;

        [StringLength(10, ErrorMessage = "La categoría de licencia no debe superar los 10 caracteres.")]
        [Display(Name = "Categoría de licencia")]
        public string? CategoriaLicencia { get; set; }

        [StringLength(20, ErrorMessage = "El teléfono no debe superar los 20 caracteres.")]
        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        [StringLength(200, ErrorMessage = "La dirección no debe superar los 200 caracteres.")]
        [Display(Name = "Dirección")]
        public string? Direccion { get; set; }

        [Display(Name = "Estado")]
        public bool Estado { get; set; } = true;

        public List<SelectListItem> Usuarios { get; set; } = new();
    }
}
