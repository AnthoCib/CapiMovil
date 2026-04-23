using CapiMovil.BL.BE;

namespace CapiMovil.PL.Gui.Models.ViewModels
{
    public class ConductorDashboardViewModel
    {
        public string NombreConductor { get; set; } = string.Empty;
        public RecorridoBE? RecorridoHoy { get; set; }
        public int RecorridosHoy { get; set; }
        public int IncidenciasAbiertas { get; set; }
        public int EstudiantesRutaActiva { get; set; }
        public List<IncidenciaBE> AlertasRecientes { get; set; } = new();
        public List<ConductorDashboardEstudianteItemViewModel> EstudiantesEnRuta { get; set; } = new();
        public bool PuedeIniciarRecorrido { get; set; }
    }

    public class ConductorDashboardEstudianteItemViewModel
    {
        public string Nombre { get; set; } = string.Empty;
        public string Parada { get; set; } = "Sin parada";
        public string Hora { get; set; } = "--:--";
        public string Estado { get; set; } = "PENDIENTE";
    }
}
