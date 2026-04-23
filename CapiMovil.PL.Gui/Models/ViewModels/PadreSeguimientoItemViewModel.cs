namespace CapiMovil.PL.Gui.Models.ViewModels
{
    public class PadreSeguimientoItemViewModel
    {
        public string CodigoEstudiante { get; set; } = string.Empty;
        public string NombreEstudiante { get; set; } = string.Empty;
        public string? Ruta { get; set; }
        public string? Paradero { get; set; }
        public string RecorridoDeHoy { get; set; } = "Sin recorrido asignado hoy";
        public string Estado { get; set; } = "Sin estado disponible";
        public string? UltimoEvento { get; set; }
        public DateTime? FechaUltimoEvento { get; set; }
    }
}
