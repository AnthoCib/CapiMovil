using System.ComponentModel.DataAnnotations;

namespace CapiMovil.PL.Gui.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Ingrese su usuario o correo.")]
        [Display(Name = "Usuario o Correo")]
        public string UsuarioOCorreo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ingrese su contraseña.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;
    }
}

