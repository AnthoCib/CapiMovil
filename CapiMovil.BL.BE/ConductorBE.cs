namespace CapiMovil.BL.BE
{
    public class ConductorBE
    {
        public Guid IdConductor { get; set; }
        public Guid IdUsuario { get; set; }

        public string CodigoConductor { get; set; } = string.Empty;
        public string Nombres { get; set; } = string.Empty;
        public string ApellidoPaterno { get; set; } = string.Empty;
        public string ApellidoMaterno { get; set; } = string.Empty;

        public string? DNI { get; set; }
        public string Licencia { get; set; } = string.Empty;
        public string? CategoriaLicencia { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }

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