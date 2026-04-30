namespace CapiMovil.PL.Gui.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public string Saludo { get; set; } = "Buenos días";
        public int TotalUsuariosActivos { get; set; }
        public int TotalPadresActivos { get; set; }
        public int TotalConductoresActivos { get; set; }
        public int TotalEstudiantes { get; set; }
        public int TotalBusesActivos { get; set; }
        public int TotalRutasActivas { get; set; }
        public int TotalRecorridosProgramadosHoy { get; set; }
        public int TotalRecorridosEnCurso { get; set; }
        public int TotalIncidenciasPendientes { get; set; }
        public int TotalNotificacionesNoLeidas { get; set; }
        public int TotalAbordajesHoy { get; set; }

        public int TotalRecorridosSinFinalizar { get; set; }
        public int TotalBusesInactivos { get; set; }
        public int TotalRutasSinParaderos { get; set; }
        public int TotalEstudiantesSinRuta { get; set; }
        public int TotalIncidenciasCriticas { get; set; }

        public List<AdminActividadItemViewModel> ActividadReciente { get; set; } = new();
        public List<AdminPendienteItemViewModel> PendientesAtencion { get; set; } = new();
    }

    public class AdminActividadItemViewModel
    {
        public string Icono { get; set; } = "bi-clock-history";
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string FechaTexto { get; set; } = string.Empty;
        public string Tipo { get; set; } = "info";
        public string EstadoTexto { get; set; } = "Informativo";
    }

    public class AdminPendienteItemViewModel
    {
        public string Icono { get; set; } = "bi-exclamation-circle";
        public string Titulo { get; set; } = string.Empty;
        public string Detalle { get; set; } = string.Empty;
        public int Total { get; set; }
        public string Tipo { get; set; } = "warning";
    }
}
