namespace CapiMovil.BL.BE
{
    public class EventoAbordajeBE
    {
        public Guid IdEvento { get; set; }
        public Guid IdRecorrido { get; set; }
        public Guid IdEstudiante { get; set; }
        public Guid? IdParadero { get; set; }
        public Guid? RegistradoPor { get; set; }

        public string CodigoEvento { get; set; } = string.Empty;
        public string TipoEvento { get; set; } = "SUBIDA";
        public DateTime FechaHora { get; set; }
        public string? Observacion { get; set; }

        public bool Estado { get; set; } = true;
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public DateTime? FechaEliminacion { get; set; }

        public RecorridoBE? Recorrido { get; set; }
        public EstudianteBE? Estudiante { get; set; }
        public ParaderoBE? Paradero { get; set; }
    }
}