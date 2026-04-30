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
        private readonly ParaderoBC _paraderoBC;
        private readonly RutaEstudianteBC _rutaEstudianteBC;
        private readonly RecorridoBC _recorridoBC;
        private readonly EventoAbordajeBC _eventoAbordajeBC;
        private readonly IncidenciaBC _incidenciaBC;
        private readonly NotificacionBC _notificacionBC;

        public AdminController(
            UsuarioBC usuarioBC,
            PadreFamiliaBC padreFamiliaBC,
            ConductorBC conductorBC,
            EstudianteBC estudianteBC,
            RutaBC rutaBC,
            BusBC busBC,
            ParaderoBC paraderoBC,
            RutaEstudianteBC rutaEstudianteBC,
            RecorridoBC recorridoBC,
            EventoAbordajeBC eventoAbordajeBC,
            UbicacionBusBC ubicacionBusBC,
            IncidenciaBC incidenciaBC,
            NotificacionBC notificacionBC)
        {
            _usuarioBC = usuarioBC;
            _padreFamiliaBC = padreFamiliaBC;
            _conductorBC = conductorBC;
            _estudianteBC = estudianteBC;
            _rutaBC = rutaBC;
            _busBC = busBC;
            _paraderoBC = paraderoBC;
            _rutaEstudianteBC = rutaEstudianteBC;
            _recorridoBC = recorridoBC;
            _eventoAbordajeBC = eventoAbordajeBC;
            _incidenciaBC = incidenciaBC;
            _notificacionBC = notificacionBC;
        }

        public IActionResult Index()
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Administracion);
            if (acceso != null)
                return acceso;

            DateTime hoy = DateTime.Today;
            List<UsuarioBE> usuarios = _usuarioBC.Listar();
            List<PadreFamiliaBE> padres = _padreFamiliaBC.Listar();
            List<ConductorBE> conductores = _conductorBC.Listar();
            List<EstudianteBE> estudiantes = _estudianteBC.Listar();
            List<BusBE> buses = _busBC.Listar();
            List<RutaBE> rutas = _rutaBC.Listar();
            List<ParaderoBE> paraderos = _paraderoBC.Listar();
            List<RutaEstudianteBE> asignaciones = _rutaEstudianteBC.Listar();
            List<RecorridoBE> recorridos = _recorridoBC.Listar();
            List<EventoAbordajeBE> abordajes = _eventoAbordajeBC.Listar();
            List<IncidenciaBE> incidencias = _incidenciaBC.Listar();
            List<NotificacionBE> notificaciones = _notificacionBC.Listar();

            var rutasConParadero = paraderos
                .Where(p => p.Ruta != null && p.Ruta.IdRuta != Guid.Empty)
                .Select(p => p.Ruta!.IdRuta)
                .Distinct()
                .ToHashSet();

            AdminDashboardViewModel vm = new()
            {
                Saludo = ObtenerSaludo(),
                TotalUsuariosActivos = usuarios.Count(x => x.Estado),
                TotalPadresActivos = padres.Count(x => x.Estado),
                TotalConductoresActivos = conductores.Count(x => x.Estado),
                TotalEstudiantes = estudiantes.Count(x => x.Estado),
                TotalBusesActivos = buses.Count(x => x.Estado),
                TotalRutasActivas = rutas.Count(x => x.Estado && string.Equals(x.EstadoRuta, "ACTIVA", StringComparison.OrdinalIgnoreCase)),
                TotalRecorridosProgramadosHoy = recorridos.Count(x => x.Fecha.Date == hoy && string.Equals(x.EstadoRecorrido, "PROGRAMADO", StringComparison.OrdinalIgnoreCase)),
                TotalRecorridosEnCurso = recorridos.Count(x => string.Equals(x.EstadoRecorrido, "EN_CURSO", StringComparison.OrdinalIgnoreCase)),
                TotalIncidenciasPendientes = incidencias.Count(x => !string.Equals(x.EstadoIncidencia, "CERRADA", StringComparison.OrdinalIgnoreCase)),
                TotalNotificacionesNoLeidas = notificaciones.Count(x => !x.Leido),
                TotalAbordajesHoy = abordajes.Count(x => x.FechaHora.Date == hoy),
                TotalRecorridosSinFinalizar = recorridos.Count(x => !string.Equals(x.EstadoRecorrido, "FINALIZADO", StringComparison.OrdinalIgnoreCase)),
                TotalBusesInactivos = buses.Count(x => !x.Estado),
                TotalRutasSinParaderos = rutas.Count(x => x.Estado && !rutasConParadero.Contains(x.IdRuta)),
                TotalEstudiantesSinRuta = estudiantes.Count(x => x.Estado && !asignaciones.Any(a => a.Estado && a.IdEstudiante == x.IdEstudiante)),
                TotalIncidenciasCriticas = incidencias.Count(x => string.Equals(x.Prioridad, "CRITICA", StringComparison.OrdinalIgnoreCase)),
                ActividadReciente = ConstruirActividadReciente(incidencias, notificaciones, recorridos, abordajes),
                PendientesAtencion = ConstruirPendientesAtencion(incidencias, recorridos, buses, rutas, estudiantes, asignaciones, rutasConParadero)
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
            List<RecorridoBE> recorridos,
            List<EventoAbordajeBE> abordajes)
        {
            List<AdminActividadItemViewModel> actividad = new();

            actividad.AddRange(incidencias.OrderByDescending(i => i.FechaHora).Take(3).Select(i => new AdminActividadItemViewModel
            {
                Icono = "bi-exclamation-triangle",
                Tipo = "danger",
                Titulo = $"Incidencia {i.TipoIncidencia}",
                Descripcion = i.Descripcion,
                Fecha = i.FechaHora,
                FechaTexto = i.FechaHora.ToString("dd/MM/yyyy HH:mm"),
                EstadoTexto = string.IsNullOrWhiteSpace(i.Prioridad) ? "Pendiente" : i.Prioridad
            }));

            actividad.AddRange(recorridos.OrderByDescending(r => r.Fecha).Take(3).Select(r => new AdminActividadItemViewModel
            {
                Icono = "bi-signpost-2",
                Tipo = "info",
                Titulo = $"Recorrido {r.CodigoRecorrido}",
                Descripcion = $"{r.Ruta?.Nombre ?? "Ruta"} - {r.Conductor?.NombreCompleto ?? "Conductor"}",
                Fecha = r.Fecha,
                FechaTexto = r.Fecha.ToString("dd/MM/yyyy HH:mm"),
                EstadoTexto = r.EstadoRecorrido ?? "Sin estado"
            }));

            actividad.AddRange(notificaciones.OrderByDescending(n => n.FechaEnvio).Take(2).Select(n => new AdminActividadItemViewModel
            {
                Icono = "bi-bell",
                Tipo = n.Leido ? "secondary" : "warning",
                Titulo = n.Titulo,
                Descripcion = n.Mensaje,
                Fecha = n.FechaEnvio,
                FechaTexto = n.FechaEnvio.ToString("dd/MM/yyyy HH:mm"),
                EstadoTexto = n.Leido ? "Leída" : "Pendiente"
            }));

            actividad.AddRange(abordajes.OrderByDescending(a => a.FechaHora).Take(2).Select(a => new AdminActividadItemViewModel
            {
                Icono = "bi-person-check",
                Tipo = "success",
                Titulo = $"Abordaje {a.TipoEvento}",
                Descripcion = a.Estudiante?.NombreCompleto ?? "Estudiante no disponible",
                Fecha = a.FechaHora,
                FechaTexto = a.FechaHora.ToString("dd/MM/yyyy HH:mm"),
                EstadoTexto = a.TipoEvento ?? "Registrado"
            }));

            return actividad.OrderByDescending(x => x.Fecha).Take(10).ToList();
        }

        private static List<AdminPendienteItemViewModel> ConstruirPendientesAtencion(
            List<IncidenciaBE> incidencias,
            List<RecorridoBE> recorridos,
            List<BusBE> buses,
            List<RutaBE> rutas,
            List<EstudianteBE> estudiantes,
            List<RutaEstudianteBE> asignaciones,
            HashSet<Guid> rutasConParadero)
        {
            return new List<AdminPendienteItemViewModel>
            {
                new() { Icono = "bi-exclamation-octagon", Tipo = "danger", Titulo = "Incidencias críticas", Detalle = "Requieren atención inmediata.", Total = incidencias.Count(x => string.Equals(x.Prioridad, "CRITICA", StringComparison.OrdinalIgnoreCase)) },
                new() { Icono = "bi-clock-history", Tipo = "warning", Titulo = "Recorridos en curso", Detalle = "Verificar avance y cumplimiento.", Total = recorridos.Count(x => string.Equals(x.EstadoRecorrido, "EN_CURSO", StringComparison.OrdinalIgnoreCase)) },
                new() { Icono = "bi-sign-stop", Tipo = "secondary", Titulo = "Recorridos sin finalizar", Detalle = "Incluye recorridos no finalizados.", Total = recorridos.Count(x => !string.Equals(x.EstadoRecorrido, "FINALIZADO", StringComparison.OrdinalIgnoreCase)) },
                new() { Icono = "bi-bus-front", Tipo = "secondary", Titulo = "Buses inactivos", Detalle = "Unidades fuera de operación.", Total = buses.Count(x => !x.Estado) },
                new() { Icono = "bi-geo-alt", Tipo = "info", Titulo = "Rutas sin paraderos", Detalle = "Rutas activas pendientes de puntos.", Total = rutas.Count(x => x.Estado && !rutasConParadero.Contains(x.IdRuta)) },
                new() { Icono = "bi-person-x", Tipo = "warning", Titulo = "Estudiantes sin ruta", Detalle = "Requieren asignación de ruta.", Total = estudiantes.Count(x => x.Estado && !asignaciones.Any(a => a.Estado && a.IdEstudiante == x.IdEstudiante)) }
            };
        }
    }
}
