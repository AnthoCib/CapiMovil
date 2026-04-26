namespace CapiMovil.PL.Gui.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public string Saludo { get; set; } = "Buenos días";
        public int TotalEstudiantes { get; set; }
        public int TotalConductoresActivos { get; set; }
        public int TotalRutasActivas { get; set; }
        public int TotalRecorridosEnCurso { get; set; }
        public int TotalIncidenciasActivas { get; set; }
        public int TotalNotificacionesPendientes { get; set; }
        public int TotalBusesActivos { get; set; }
        public int TotalPadresActivos { get; set; }
        public int TotalUsuariosActivos { get; set; }
        public int TotalParaderos { get; set; }
        public int TotalAsignacionesRutaEstudiante { get; set; }
        public int TotalAbordajesHoy { get; set; }
        public int TotalNotificacionesRegistradas { get; set; }
        public List<string> RecorridosTendenciaLabels { get; set; } = new();
        public List<int> RecorridosTendenciaData { get; set; } = new();
        public List<int> IncidenciasTendenciaData { get; set; } = new();
        public List<string> RecorridosEstadoLabels { get; set; } = new();
        public List<int> RecorridosEstadoData { get; set; } = new();
        public List<string> IncidenciasEstadoLabels { get; set; } = new();
        public List<int> IncidenciasEstadoData { get; set; } = new();
        public List<string> IncidenciasPrioridadLabels { get; set; } = new();
        public List<int> IncidenciasPrioridadData { get; set; } = new();
        public List<AdminMapaBusItemViewModel> BusesMapa { get; set; } = new();
        public List<AdminMapaParaderoItemViewModel> ParaderosMapa { get; set; } = new();
        public List<AdminActividadItemViewModel> ActividadReciente { get; set; } = new();
    }

    public class AdminActividadItemViewModel
    {
        public string Icono { get; set; } = "bi-clock-history";
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string FechaTexto { get; set; } = string.Empty;
        public string Tipo { get; set; } = "info";
    }

    public class AdminMapaBusItemViewModel
    {
        public decimal Latitud { get; set; }
        public decimal Longitud { get; set; }
        public string CodigoRecorrido { get; set; } = string.Empty;
        public string Ruta { get; set; } = "Ruta no disponible";
        public string Conductor { get; set; } = "Conductor no disponible";
        public string Bus { get; set; } = "Bus no disponible";
        public string EstadoRecorrido { get; set; } = "SIN_ESTADO";
        public DateTime FechaHora { get; set; }
    }

    public class AdminMapaParaderoItemViewModel
    {
        public decimal Latitud { get; set; }
        public decimal Longitud { get; set; }
        public string Nombre { get; set; } = "Paradero";
        public string Ruta { get; set; } = "Ruta no disponible";
    }
}
