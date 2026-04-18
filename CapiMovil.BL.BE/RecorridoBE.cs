namespace CapiMovil.BL.BE
{
    public class RecorridoBE
    {
        public Guid IdRecorrido { get; set; }
        public Guid IdRuta { get; set; }
        public Guid IdBus { get; set; }
        public Guid IdConductor { get; set; }

        public string CodigoRecorrido { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }

        public TimeSpan? HoraInicioProgramada { get; set; }
        public TimeSpan? HoraFinProgramada { get; set; }

        public DateTime? HoraInicioReal { get; set; }
        public DateTime? HoraFinReal { get; set; }

        public string EstadoRecorrido { get; set; } = "PROGRAMADO";
        public string? Observaciones { get; set; }

        public bool Estado { get; set; } = true;
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public DateTime? FechaEliminacion { get; set; }

        public RutaBE? Ruta { get; set; }
        public BusBE? Bus { get; set; }
        public ConductorBE? Conductor { get; set; }
    }
}