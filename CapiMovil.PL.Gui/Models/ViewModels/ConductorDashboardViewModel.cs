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
    }
}
