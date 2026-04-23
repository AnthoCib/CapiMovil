namespace CapiMovil.PL.Gui.Models.ViewModels
{
    public class PadreDashboardViewModel
    {
        public string NombrePadre { get; set; } = "Padre de Familia";
        public int CantidadHijos { get; set; }
        public int CantidadNotificacionesPendientes { get; set; }
        public int CantidadEventosRecientes { get; set; }
    }
}
