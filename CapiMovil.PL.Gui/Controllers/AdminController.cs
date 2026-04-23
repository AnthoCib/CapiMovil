using CapiMovil.BL.BC;
using CapiMovil.BL.BE;
using CapiMovil.PL.Gui.Infrastructure;
using CapiMovil.PL.Gui.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace CapiMovil.PL.Gui.Controllers
{
    public class AdminController : Controller
    {
        private readonly UsuarioBC _usuarioBC;
        private readonly PadreFamiliaBC _padreFamiliaBC;
        private readonly ConductorBC _conductorBC;
        private readonly EstudianteBC _estudianteBC;
        private readonly RutaBC _rutaBC;
        private readonly BusBC _busBC;
        private readonly RecorridoBC _recorridoBC;
        private readonly IncidenciaBC _incidenciaBC;
        private readonly NotificacionBC _notificacionBC;

        public AdminController(
            UsuarioBC usuarioBC,
            PadreFamiliaBC padreFamiliaBC,
            ConductorBC conductorBC,
            EstudianteBC estudianteBC,
            RutaBC rutaBC,
            BusBC busBC,
            RecorridoBC recorridoBC,
            IncidenciaBC incidenciaBC,
            NotificacionBC notificacionBC)
        {
            _usuarioBC = usuarioBC;
            _padreFamiliaBC = padreFamiliaBC;
            _conductorBC = conductorBC;
            _estudianteBC = estudianteBC;
            _rutaBC = rutaBC;
            _busBC = busBC;
            _recorridoBC = recorridoBC;
            _incidenciaBC = incidenciaBC;
            _notificacionBC = notificacionBC;
        }

        public IActionResult Index()
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Administracion);
            if (acceso != null)
                return acceso;

            List<IncidenciaBE> incidencias = _incidenciaBC.Listar();
            List<NotificacionBE> notificaciones = _notificacionBC.Listar();
            List<RecorridoBE> recorridos = _recorridoBC.Listar();

            AdminDashboardViewModel vm = new()
            {
                Saludo = ObtenerSaludo(),
                TotalUsuariosActivos = _usuarioBC.Listar().Count(x => x.Estado),
                TotalPadresActivos = _padreFamiliaBC.Listar().Count(x => x.Estado),
                TotalConductoresActivos = _conductorBC.Listar().Count(x => x.Estado),
                TotalEstudiantes = _estudianteBC.Listar().Count(x => x.Estado),
                TotalBusesActivos = _busBC.Listar().Count(x => x.Estado),
                TotalRutasActivas = _rutaBC.Listar().Count(x =>
                    x.Estado && string.Equals(x.EstadoRuta, "ACTIVA", StringComparison.OrdinalIgnoreCase)),
                TotalRecorridosEnCurso = recorridos.Count(x =>
                    string.Equals(x.EstadoRecorrido, "EN_CURSO", StringComparison.OrdinalIgnoreCase)),
                TotalIncidenciasActivas = incidencias.Count(x =>
                    !string.Equals(x.EstadoIncidencia, "CERRADA", StringComparison.OrdinalIgnoreCase)),
                TotalNotificacionesPendientes = notificaciones.Count(x => !x.Leido),
                ActividadReciente = ConstruirActividadReciente(incidencias, notificaciones, recorridos)
            };

            return View(vm);
        }

        private static string ObtenerSaludo()
        {
            int hora = DateTime.Now.Hour;
            if (hora < 12) return "Buenos días";
            if (hora < 19) return "Buenas tardes";
            return "Buenas noches";
        }

        private static List<AdminActividadItemViewModel> ConstruirActividadReciente(
            List<IncidenciaBE> incidencias,
            List<NotificacionBE> notificaciones,
            List<RecorridoBE> recorridos)
        {
            List<AdminActividadItemViewModel> actividad = new();

            actividad.AddRange(
                incidencias
                    .OrderByDescending(i => i.FechaHora)
                    .Take(3)
                    .Select(i => new AdminActividadItemViewModel
                    {
                        Icono = "bi-exclamation-triangle",
                        Tipo = "danger",
                        Titulo = $"Incidencia {i.TipoIncidencia}",
                        Descripcion = i.Descripcion,
                        Fecha = i.FechaHora,
                        FechaTexto = i.FechaHora.ToString("dd/MM/yyyy HH:mm")
                    }));

            actividad.AddRange(
                notificaciones
                    .OrderByDescending(n => n.FechaEnvio)
                    .Take(3)
                    .Select(n => new AdminActividadItemViewModel
                    {
                        Icono = "bi-bell",
                        Tipo = n.Leido ? "secondary" : "warning",
                        Titulo = n.Titulo,
                        Descripcion = n.Mensaje,
                        Fecha = n.FechaEnvio,
                        FechaTexto = n.FechaEnvio.ToString("dd/MM/yyyy HH:mm")
                    }));

            actividad.AddRange(
                recorridos
                    .OrderByDescending(r => r.Fecha)
                    .Take(3)
                    .Select(r => new AdminActividadItemViewModel
                    {
                        Icono = "bi-signpost-2",
                        Tipo = "info",
                        Titulo = $"Recorrido {r.CodigoRecorrido}",
                        Descripcion = $"{r.Ruta?.Nombre ?? "Ruta"} - {r.EstadoRecorrido}",
                        Fecha = r.Fecha,
                        FechaTexto = r.Fecha.ToString("dd/MM/yyyy")
                    }));

            return actividad
                .OrderByDescending(x => x.Fecha)
                .Take(8)
                .ToList();
        }
    }
}
