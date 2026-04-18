using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace CapiMovil.PL.Gui.Models.ViewModels
{
    public class PadreFamiliaFormViewModel
    {
        public Guid IdPadre { get; set; }

        [Display(Name = "Usuario")]
        public Guid IdUsuario { get; set; }

        
        [Display(Name = "Código")]
        public string CodigoPadre { get; set; } = string.Empty;

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

        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        [Display(Name = "Teléfono alterno")]
        public string? TelefonoAlterno { get; set; }

        [Display(Name = "Dirección")]
        public string? Direccion { get; set; }

        [EmailAddress(ErrorMessage = "Ingrese un correo válido.")]
        [Display(Name = "Correo de contacto")]
        public string? CorreoContacto { get; set; }

        [Display(Name = "Estado")]
        public bool Estado { get; set; } = true;

        public List<SelectListItem> Usuarios { get; set; } = new();
    }
}