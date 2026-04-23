using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace CapiMovil.PL.Gui.Models.ViewModels
{
    public class PadreFamiliaFormViewModel
    {
        public Guid IdPadre { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un usuario.")]
        [Display(Name = "Usuario")]
        public Guid IdUsuario { get; set; }

        [Display(Name = "Código")]
        public string CodigoPadre { get; set; } = string.Empty;

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

        [StringLength(20, ErrorMessage = "El teléfono no debe superar los 20 caracteres.")]
        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        [StringLength(20, ErrorMessage = "El teléfono alterno no debe superar los 20 caracteres.")]
        [Display(Name = "Teléfono alterno")]
        public string? TelefonoAlterno { get; set; }

        [StringLength(200, ErrorMessage = "La dirección no debe superar los 200 caracteres.")]
        [Display(Name = "Dirección")]
        public string? Direccion { get; set; }

        [EmailAddress(ErrorMessage = "Ingrese un correo válido.")]
        [StringLength(120, ErrorMessage = "El correo no debe superar los 120 caracteres.")]
        [Display(Name = "Correo de contacto")]
        public string? CorreoContacto { get; set; }

        [Display(Name = "Estado")]
        public bool Estado { get; set; } = true;

        public List<SelectListItem> Usuarios { get; set; } = new();
    }
}
