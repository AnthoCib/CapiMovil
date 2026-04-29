namespace CapiMovil.PL.Gui.Models.ViewModels
{
    public class EstudianteRutaEstadoViewModel
    {
        public Guid IdEstudiante { get; set; }

        public string Nombres { get; set; } = string.Empty;

        public string ApellidoPaterno { get; set; } = string.Empty;

        public string ApellidoMaterno { get; set; } = string.Empty;

        public string Grado { get; set; } = string.Empty;

        public string Seccion { get; set; } = string.Empty;

        public string EstadoEvento { get; set; } = "PENDIENTE";

        public string NombreCompleto
        {
            get
            {
                return $"{Nombres} {ApellidoPaterno} {ApellidoMaterno}".Trim();
            }
        }
    }
}