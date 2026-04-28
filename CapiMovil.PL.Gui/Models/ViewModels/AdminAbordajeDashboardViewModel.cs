using Microsoft.AspNetCore.Mvc.Rendering;

namespace CapiMovil.PL.Gui.Models.ViewModels
{
    public class AdminAbordajeDashboardViewModel
    {
        public DateTime Fecha { get; set; } = DateTime.Today;
        public Guid? IdRuta { get; set; }
        public Guid? IdRecorrido { get; set; }
        public Guid? IdConductor { get; set; }
        public string? EstadoRecorrido { get; set; }
        public string? EstadoAbordaje { get; set; }

        public int TotalRecorridosDia { get; set; }
        public int TotalEstudiantesAsignados { get; set; }
        public int TotalAbordajesRegistrados { get; set; }
        public int TotalSubidas { get; set; }
        public int TotalBajadas { get; set; }
        public int TotalAusentes { get; set; }
        public int TotalNoAbordo { get; set; }
        public int TotalIncidenciasRelacionadas { get; set; }

        public AdminAbordajeRecorridoDetalleViewModel? RecorridoSeleccionado { get; set; }
        public List<AdminAbordajeParaderoViewModel> Paraderos { get; set; } = new();
        public List<AdminAbordajeTimelineItemViewModel> Timeline { get; set; } = new();
        public List<AdminAbordajeBusMapaItemViewModel> BusesMapa { get; set; } = new();

        public List<SelectListItem> Rutas { get; set; } = new();
        public List<SelectListItem> Recorridos { get; set; } = new();
        public List<SelectListItem> Conductores { get; set; } = new();
        public List<SelectListItem> EstadosRecorrido { get; set; } = new();
        public List<SelectListItem> EstadosAbordaje { get; set; } = new();
    }

    public class AdminAbordajeRecorridoDetalleViewModel
    {
        public Guid IdRecorrido { get; set; }
        public string CodigoRecorrido { get; set; } = string.Empty;
        public string Ruta { get; set; } = string.Empty;
        public string Bus { get; set; } = string.Empty;
        public string Conductor { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public TimeSpan? HoraInicioProgramada { get; set; }
        public DateTime? HoraInicioReal { get; set; }
        public DateTime? HoraFinReal { get; set; }
        public string EstadoRecorrido { get; set; } = string.Empty;
        public decimal? LatitudInicioRuta { get; set; }
        public decimal? LongitudInicioRuta { get; set; }
        public decimal? LatitudFinRuta { get; set; }
        public decimal? LongitudFinRuta { get; set; }
    }

    public class AdminAbordajeParaderoViewModel
    {
        public Guid IdParadero { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public int OrdenParada { get; set; }
        public TimeSpan? HoraEstimada { get; set; }
        public decimal? Latitud { get; set; }
        public decimal? Longitud { get; set; }
        public int TotalEstudiantes { get; set; }
        public int Subidas { get; set; }
        public int Bajadas { get; set; }
        public int Ausentes { get; set; }
        public int Pendientes { get; set; }
        public int NoAbordo { get; set; }
    }

    public class AdminAbordajeTimelineItemViewModel
    {
        public DateTime FechaHora { get; set; }
        public string Alumno { get; set; } = string.Empty;
        public string TipoEvento { get; set; } = string.Empty;
        public string Paradero { get; set; } = string.Empty;
        public string Recorrido { get; set; } = string.Empty;
    }

    public class AdminAbordajeBusMapaItemViewModel
    {
        public decimal Latitud { get; set; }
        public decimal Longitud { get; set; }
        public string Bus { get; set; } = string.Empty;
        public string Conductor { get; set; } = string.Empty;
        public string Recorrido { get; set; } = string.Empty;
        public DateTime FechaHora { get; set; }
    }
}
