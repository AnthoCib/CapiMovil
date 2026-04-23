using CapiMovil.BL.BE;

namespace CapiMovil.PL.Gui.Models.ViewModels
{
    public class PadreHijoDetalleViewModel
    {
        public EstudianteBE? Estudiante { get; set; }
        public RutaEstudianteBE? RutaAsignada { get; set; }
        public ParaderoBE? ParaderoSubida { get; set; }
        public ParaderoBE? ParaderoBajada { get; set; }
        public List<EventoAbordajeBE> EventosRecientes { get; set; } = new();
    }
}
