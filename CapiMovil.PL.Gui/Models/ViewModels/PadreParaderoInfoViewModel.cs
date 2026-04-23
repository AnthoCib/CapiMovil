namespace CapiMovil.PL.Gui.Models.ViewModels
{
    public class PadreParaderoInfoViewModel
    {
        public string CodigoEstudiante { get; set; } = string.Empty;
        public string NombreEstudiante { get; set; } = string.Empty;
        public string? ParaderoSubida { get; set; }
        public string? DireccionSubida { get; set; }
        public string? ParaderoBajada { get; set; }
        public string? DireccionBajada { get; set; }
    }
}
