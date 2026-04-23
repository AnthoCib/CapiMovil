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
}
