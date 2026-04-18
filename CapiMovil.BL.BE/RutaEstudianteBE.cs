namespace CapiMovil.BL.BE
{
    public class RutaEstudianteBE
    {
        public Guid IdRutaEstudiante { get; set; }
        public Guid IdRuta { get; set; }
        public Guid IdEstudiante { get; set; }
        public Guid? IdParaderoSubida { get; set; }
        public Guid? IdParaderoBajada { get; set; }

        public string CodigoRutaEstudiante { get; set; } = string.Empty;
        public DateTime FechaInicioVigencia { get; set; }
        public DateTime? FechaFinVigencia { get; set; }
        public string EstadoAsignacion { get; set; } = "ACTIVO";
        public string? Observaciones { get; set; }

        public bool Estado { get; set; } = true;
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public DateTime? FechaEliminacion { get; set; }

        public RutaBE? Ruta { get; set; }
        public EstudianteBE? Estudiante { get; set; }
        public ParaderoBE? ParaderoSubida { get; set; }
        public ParaderoBE? ParaderoBajada { get; set; }
    }
}