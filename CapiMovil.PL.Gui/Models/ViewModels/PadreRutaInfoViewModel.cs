namespace CapiMovil.PL.Gui.Models.ViewModels
{
    public class PadreRutaInfoViewModel
    {
        public string CodigoEstudiante { get; set; } = string.Empty;
        public string NombreEstudiante { get; set; } = string.Empty;
        public string CodigoRuta { get; set; } = string.Empty;
        public string NombreRuta { get; set; } = string.Empty;
        public string? CodigoRecorrido { get; set; }
        public string? EstadoRecorrido { get; set; }
        public string? PlacaBus { get; set; }
        public string? NombreConductor { get; set; }
        public DateTime? FechaRecorrido { get; set; }
    }
}
