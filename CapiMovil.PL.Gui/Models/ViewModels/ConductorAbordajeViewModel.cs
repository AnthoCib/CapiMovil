using CapiMovil.BL.BE;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CapiMovil.PL.Gui.Models.ViewModels
{
    public class ConductorAbordajeViewModel
    {
        public Guid IdEstudiante { get; set; }
        public Guid? IdParadero { get; set; }
        public string TipoEvento { get; set; } = "SUBIDA";
        public DateTime FechaHora { get; set; } = DateTime.Now;
        public string? Observacion { get; set; }

        public RecorridoBE? RecorridoActivo { get; set; }
        public List<EventoAbordajeBE> Eventos { get; set; } = new();
        public List<SelectListItem> Estudiantes { get; set; } = new();
        public List<SelectListItem> Paraderos { get; set; } = new();
        public List<SelectListItem> TiposEvento { get; set; } = new();
    }
}
