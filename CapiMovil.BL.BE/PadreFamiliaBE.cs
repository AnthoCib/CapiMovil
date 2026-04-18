namespace CapiMovil.BL.BE
{
    public class PadreFamiliaBE
    {
        public Guid IdPadre { get; set; }
        public Guid IdUsuario { get; set; }

        public string CodigoPadre { get; set; } = string.Empty;
        public string Nombres { get; set; } = string.Empty;
        public string ApellidoPaterno { get; set; } = string.Empty;
        public string ApellidoMaterno { get; set; } = string.Empty;

        public string? DNI { get; set; }
        public string? Telefono { get; set; }
        public string? TelefonoAlterno { get; set; }
        public string? Direccion { get; set; }
        public string? CorreoContacto { get; set; }

        public bool Estado { get; set; }

        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public DateTime? FechaEliminacion { get; set; }

        public UsuarioBE? Usuario { get; set; }

        public string NombreCompleto
        {
            get
            {
                return $"{Nombres} {ApellidoPaterno} {ApellidoMaterno}".Trim();
            }
        }
    }
}