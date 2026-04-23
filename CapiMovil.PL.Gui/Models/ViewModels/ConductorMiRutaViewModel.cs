using CapiMovil.BL.BE;

namespace CapiMovil.PL.Gui.Models.ViewModels
{
    public class ConductorMiRutaViewModel
    {
        public RecorridoBE? RecorridoOperacion { get; set; }
        public List<RecorridoBE> Recorridos { get; set; } = new();
        public List<ParaderoBE> Paraderos { get; set; } = new();
        public UbicacionBusBE? UltimaUbicacionBus { get; set; }

        public bool TieneParaderosConCoordenadas =>
            Paraderos.Any(p => p.Latitud.HasValue && p.Longitud.HasValue);
    }
}
