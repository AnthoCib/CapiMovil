using CapiMovil.BL.BE;

namespace CapiMovil.PL.Gui.Models.ViewModels
{
    public class PadreMapaEnVivoViewModel
    {
        public EstudianteBE? Estudiante { get; set; }
        public List<EstudianteBE> HijosDisponibles { get; set; } = new();
        public RutaEstudianteBE? RutaAsignada { get; set; }
        public RecorridoBE? RecorridoActivo { get; set; }
        public EventoAbordajeBE? UltimoEvento { get; set; }
        public ParaderoBE? ProximaParada { get; set; }
        public string? Conductor { get; set; }
        public string? Bus { get; set; }
        public string? ETA { get; set; }
        public string EstadoSeguridad { get; set; } = "Sin datos";
        public string MensajeEstado { get; set; } = "No hay rastreo en vivo disponible.";
        public bool TieneRastreoDisponible { get; set; }
    }

    public class PadreHistorialViewModel
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public int TotalViajes { get; set; }
        public int ViajesSeguros { get; set; }
        public List<PadreHistorialViajeItemViewModel> Viajes { get; set; } = new();
    }

    public class PadreHistorialViajeItemViewModel
    {
        public Guid IdEstudiante { get; set; }
        public string Estudiante { get; set; } = string.Empty;
        public DateTime? Fecha { get; set; }
        public string TipoRecorrido { get; set; } = "Sin tipo";
        public string Ruta { get; set; } = "No disponible";
        public string Hora { get; set; } = "--:--";
        public string Estado { get; set; } = "Sin estado";
        public string Conductor { get; set; } = "No disponible";
        public bool TieneIncidencia { get; set; }
    }
}
