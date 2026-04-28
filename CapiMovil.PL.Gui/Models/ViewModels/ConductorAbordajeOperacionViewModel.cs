using Microsoft.AspNetCore.Mvc.Rendering;

namespace CapiMovil.PL.Gui.Models.ViewModels
{
    public class ConductorAbordajeOperacionViewModel
    {
        public Guid? IdRecorridoSeleccionado { get; set; }
        public Guid? IdParaderoFiltro { get; set; }
        public string EstadoFiltro { get; set; } = "PENDIENTE";

        public string CodigoRecorrido { get; set; } = "Sin recorrido";
        public string Ruta { get; set; } = "Sin ruta";
        public string Bus { get; set; } = "Sin bus";
        public string Conductor { get; set; } = "Sin conductor";
        public DateTime FechaOperacion { get; set; } = DateTime.Today;
        public string EstadoRecorrido { get; set; } = "SIN_ESTADO";
        public string? ParaderoActual { get; set; }

        public int TotalAlumnos { get; set; }
        public int TotalSubieron { get; set; }
        public int TotalBajaron { get; set; }
        public int TotalPendientes { get; set; }
        public int TotalAusentes { get; set; }
        public int TotalNoAbordo { get; set; }

        public List<SelectListItem> Recorridos { get; set; } = new();
        public List<SelectListItem> ParaderosFiltro { get; set; } = new();
        public List<SelectListItem> EstadosFiltro { get; set; } = new();
        public List<ConductorAbordajeAlumnoItemViewModel> Alumnos { get; set; } = new();
    }

    public class ConductorAbordajeAlumnoItemViewModel
    {
        public Guid IdEstudiante { get; set; }
        public string CodigoEstudiante { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string GradoSeccion { get; set; } = "No registrado";
        public Guid? IdParaderoSubida { get; set; }
        public string ParaderoSubida { get; set; } = "Sin paradero";
        public int OrdenParaderoSubida { get; set; }

        public string EstadoActual { get; set; } = "PENDIENTE";
        public string EstadoBadgeClass { get; set; } = "bg-warning text-dark";
        public bool PermiteSubida { get; set; }
        public bool PermiteBajada { get; set; }
        public bool PermiteAusente { get; set; }
        public bool PermiteNoAbordo { get; set; }

        public DateTime? UltimoEventoFechaHora { get; set; }
    }
}
