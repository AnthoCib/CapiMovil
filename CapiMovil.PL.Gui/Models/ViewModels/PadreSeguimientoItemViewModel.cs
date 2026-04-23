namespace CapiMovil.PL.Gui.Models.ViewModels
{
    public class PadreSeguimientoItemViewModel
    {
        public string CodigoEstudiante { get; set; } = string.Empty;
        public string NombreEstudiante { get; set; } = string.Empty;
        public string? Ruta { get; set; }
        public string? Paradero { get; set; }
        public string? RecorridoHoy { get; set; }
        public string? EstadoRecorrido { get; set; }
        public string? UltimoEvento { get; set; }
        public DateTime? FechaUltimoEvento { get; set; }
    }
}
