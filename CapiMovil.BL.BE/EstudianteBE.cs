namespace CapiMovil.BL.BE
{
    public class EstudianteBE
    {
        public Guid IdEstudiante { get; set; }
        public Guid IdPadre { get; set; }

        public string CodigoEstudiante { get; set; } = string.Empty;
        public string Nombres { get; set; } = string.Empty;
        public string ApellidoPaterno { get; set; } = string.Empty;
        public string ApellidoMaterno { get; set; } = string.Empty;

        public string? DNI { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string? Genero { get; set; }
        public string? Grado { get; set; }
        public string? Seccion { get; set; }
        public string? Direccion { get; set; }

        public decimal? LatitudCasa { get; set; }
        public decimal? LongitudCasa { get; set; }
        public string? FotoUrl { get; set; }
        public string? Observaciones { get; set; }

        public bool Estado { get; set; }

        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public DateTime? FechaEliminacion { get; set; }

        public PadreFamiliaBE? PadreFamilia { get; set; }

        public string NombreCompleto
        {
            get
            {
                return $"{Nombres} {ApellidoPaterno} {ApellidoMaterno}".Trim();
            }
        }
    }
}