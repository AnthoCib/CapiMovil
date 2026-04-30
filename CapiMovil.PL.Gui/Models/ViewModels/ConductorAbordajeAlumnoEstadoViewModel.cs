namespace CapiMovil.PL.Gui.Models.ViewModels
{
    public class ConductorAbordajeAlumnoEstadoViewModel
    {
        public Guid IdEstudiante { get; set; }
        public string NombreEstudiante { get; set; } = string.Empty;
        public int TotalSubidas { get; set; }
        public int TotalBajadas { get; set; }
        public int TotalAusentes { get; set; }
        public int TotalNoAbordo { get; set; }
        public string EstadoActual { get; set; } = "SIN_EVENTOS";
        public bool PermiteSubida { get; set; }
        public bool PermiteBajada { get; set; }
        public bool PermiteAusente { get; set; }
        public bool PermiteNoAbordo { get; set; }
    }
}
