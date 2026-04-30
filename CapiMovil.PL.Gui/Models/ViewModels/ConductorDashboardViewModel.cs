using CapiMovil.BL.BE;

namespace CapiMovil.PL.Gui.Models.ViewModels
{
    public class ConductorDashboardViewModel
    {
        public string NombreConductor { get; set; } = string.Empty;

        public RecorridoBE? RecorridoActual { get; set; }

        public List<RecorridoResumenConductorViewModel> RecorridosHoy { get; set; } = new List<RecorridoResumenConductorViewModel>();

        public List<EstudianteRutaEstadoBE> EstudiantesEnRuta { get; set; } = new List<EstudianteRutaEstadoBE>();

        public List<ParaderoResumenConductorViewModel> ParaderosRecojo { get; set; } = new List<ParaderoResumenConductorViewModel>();

        public List<ParaderoResumenConductorViewModel> ParaderosEntrega { get; set; } = new List<ParaderoResumenConductorViewModel>();

        public List<IncidenciaResumenConductorViewModel> IncidenciasPendientes { get; set; } = new List<IncidenciaResumenConductorViewModel>();

        public Guid? IdRecorridoOperacion { get; set; }

        public Guid? IdParaderoActual { get; set; }

        public string TipoParaderoActual { get; set; } = string.Empty;

        public string NombreParaderoActual { get; set; } = string.Empty;

        public List<EstudianteBE> EstudiantesParaderoActual { get; set; } = new List<EstudianteBE>();

        public bool MostrarTablaParadero
        {
            get
            {
                return IdRecorridoOperacion.HasValue
                    && IdParaderoActual.HasValue
                    && EstudiantesParaderoActual.Any();
            }
        }

        public int TotalEstudiantes
        {
            get
            {
                return EstudiantesEnRuta.Count;
            }
        }
    }

    public class RecorridoResumenConductorViewModel
    {
        public Guid IdRecorrido { get; set; }

        public string CodigoRecorrido { get; set; } = string.Empty;

        public string NombreRuta { get; set; } = string.Empty;

        public string CodigoBus { get; set; } = string.Empty;

        public TimeSpan? HoraInicioProgramada { get; set; }

        public TimeSpan? HoraFinProgramada { get; set; }

        public string EstadoRecorrido { get; set; } = string.Empty;

        public string TurnoTexto { get; set; } = string.Empty;
    }

    public class ParaderoResumenConductorViewModel
    {
        public Guid IdParadero { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Direccion { get; set; } = string.Empty;

        public int OrdenParada { get; set; }

        public TimeSpan? HoraEstimada { get; set; }

        public int TotalAlumnos { get; set; }

        public int TotalRegistrados { get; set; }

        public bool Completado { get; set; }
    }

    public class IncidenciaResumenConductorViewModel
    {
        public string TipoIncidencia { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        public string Prioridad { get; set; } = string.Empty;

        public DateTime FechaHora { get; set; }
    }
}
