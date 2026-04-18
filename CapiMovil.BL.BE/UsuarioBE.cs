namespace CapiMovil.BL.BE
{
    public class UsuarioBE
    {
        public Guid IdUsuario { get; set; }
        public Guid IdRol { get; set; }

        public string CodigoUsuario { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        public DateTime? UltimoAcceso { get; set; }
        public bool Estado { get; set; }

        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public DateTime? FechaEliminacion { get; set; }

        public RolBE? Rol { get; set; }

    }
}
