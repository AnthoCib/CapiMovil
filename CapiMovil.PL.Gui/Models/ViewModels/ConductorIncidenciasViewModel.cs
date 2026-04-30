using CapiMovil.BL.BE;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CapiMovil.PL.Gui.Models.ViewModels
{
    public class ConductorIncidenciasViewModel
    {
        public string TipoIncidencia { get; set; } = "OTRO";
        public string Descripcion { get; set; } = string.Empty;
        public DateTime FechaHora { get; set; } = DateTime.Now;
        public string Prioridad { get; set; } = "MEDIA";

        public RecorridoBE? RecorridoOperacion { get; set; }
        public List<IncidenciaBE> Incidencias { get; set; } = new();
        public List<SelectListItem> TiposIncidencia { get; set; } = new();
        public List<SelectListItem> Prioridades { get; set; } = new();
    }
}
