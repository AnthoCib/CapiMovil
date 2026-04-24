using System;
using System.ComponentModel.DataAnnotations;

namespace CapiMovil.PL.Gui.Models.ViewModels
{
    public class PerfilViewModel
    {
        public Guid IdUsuario { get; set; }
        public Guid? IdEntidadRol { get; set; }
        public string Rol { get; set; } = string.Empty;
        public string CodigoPerfil { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? TelefonoAlterno { get; set; }
        public string? Direccion { get; set; }
        public string? Documento { get; set; }
        public string? Licencia { get; set; }
        public string? CategoriaLicencia { get; set; }
        public string? CorreoContacto { get; set; }
        public bool Estado { get; set; }
        public string? FotoPerfilUrl { get; set; }
        public string InicialesAvatar { get; set; } = "CM";
        public bool EsConductor => string.Equals(Rol?.Trim(), "CONDUCTOR", StringComparison.OrdinalIgnoreCase);
        public bool EsPadre => string.Equals(Rol?.Trim(), "PADRE", StringComparison.OrdinalIgnoreCase)
                               || string.Equals(Rol?.Trim(), "PADRE DE FAMILIA", StringComparison.OrdinalIgnoreCase);
    }

    public class PerfilEditarViewModel
    {
        public Guid IdUsuario { get; set; }
        public Guid? IdEntidadRol { get; set; }
        public string Rol { get; set; } = string.Empty;

        [Required]
        [StringLength(120)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(120)]
        public string Correo { get; set; } = string.Empty;

        [StringLength(80)]
        public string? Nombres { get; set; }

        [StringLength(60)]
        public string? ApellidoPaterno { get; set; }

        [StringLength(60)]
        public string? ApellidoMaterno { get; set; }

        [StringLength(20)]
        public string? Telefono { get; set; }

        [StringLength(20)]
        public string? TelefonoAlterno { get; set; }

        [StringLength(200)]
        public string? Direccion { get; set; }

        [EmailAddress]
        [StringLength(120)]
        public string? CorreoContacto { get; set; }

        [StringLength(30)]
        public string? Licencia { get; set; }

        [StringLength(10)]
        public string? CategoriaLicencia { get; set; }

        public bool Estado { get; set; }
        public string? FotoPerfilUrl { get; set; }
        public string InicialesAvatar { get; set; } = "CM";
        public bool EsConductor => string.Equals(Rol?.Trim(), "CONDUCTOR", StringComparison.OrdinalIgnoreCase);
        public bool EsPadre => string.Equals(Rol?.Trim(), "PADRE", StringComparison.OrdinalIgnoreCase)
                               || string.Equals(Rol?.Trim(), "PADRE DE FAMILIA", StringComparison.OrdinalIgnoreCase);
    }
}
