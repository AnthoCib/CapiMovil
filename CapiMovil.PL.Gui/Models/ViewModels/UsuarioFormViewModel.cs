using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace CapiMovil.PL.Gui.Models.ViewModels
{
    public class UsuarioFormViewModel
    {
        public Guid IdUsuario { get; set; }

        [Display(Name = "Código")]
        public string CodigoUsuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe seleccionar un rol.")]
        [Display(Name = "Rol")]
        public Guid IdRol { get; set; }

        [Required(ErrorMessage = "El username es obligatorio.")]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingrese un correo válido.")]
        [Display(Name = "Correo")]
        public string Correo { get; set; } = string.Empty;

        
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string PasswordNueva { get; set; } = string.Empty;


        [Display(Name = "Estado")]
        public bool Estado { get; set; } = true;


        public List<SelectListItem> Roles { get; set; } = new();
    }
}