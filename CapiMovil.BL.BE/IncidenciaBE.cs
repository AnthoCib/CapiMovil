namespace CapiMovil.BL.BE
{
    public class IncidenciaBE
    {
        public Guid IdIncidencia { get; set; }
        public Guid IdRecorrido { get; set; }
        public Guid IdConductor { get; set; }
        public Guid? ReportadoPor { get; set; }

        public string CodigoIncidencia { get; set; } = string.Empty;
        public string TipoIncidencia { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public DateTime FechaHora { get; set; }

        public string EstadoIncidencia { get; set; } = string.Empty;
        public string Prioridad { get; set; } = string.Empty;

        public DateTime? FechaCierre { get; set; }
        public string? Solucion { get; set; }

        public bool Estado { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public DateTime? FechaEliminacion { get; set; }

        // Campos de apoyo para vistas
        public string? CodigoRecorrido { get; set; }
        public string? NombreConductor { get; set; }
        public string? UsernameReportadoPor { get; set; }
    }
}