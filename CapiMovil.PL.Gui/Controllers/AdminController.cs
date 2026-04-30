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
        private readonly UbicacionBusBC _ubicacionBusBC;
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
            _ubicacionBusBC = ubicacionBusBC;
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
            List<EventoAbordajeBE> abordajes = _eventoAbordajeBC.Listar();
            List<ParaderoBE> paraderos = _paraderoBC.Listar();
            List<RutaEstudianteBE> asignaciones = _rutaEstudianteBC.Listar();
            List<UbicacionBusBE> ubicaciones = _ubicacionBusBC.Listar();
            DateTime hoy = DateTime.Today;
            var tendenciaDias = Enumerable.Range(0, 7).Select(offset => hoy.AddDays(-6 + offset)).ToList();

            Dictionary<Guid, RecorridoBE> recorridosDict = recorridos.ToDictionary(r => r.IdRecorrido, r => r);
            List<AdminMapaBusItemViewModel> busesMapa = ubicaciones
                .Where(u => u.Estado)
                .GroupBy(u => u.IdRecorrido)
                .Select(g => g.OrderByDescending(x => x.FechaHora).First())
                .Where(x => recorridosDict.ContainsKey(x.IdRecorrido))
                .Select(x =>
                {
                    RecorridoBE r = recorridosDict[x.IdRecorrido];
                    return new AdminMapaBusItemViewModel
                    {
                        Latitud = x.Latitud,
                        Longitud = x.Longitud,
                        FechaHora = x.FechaHora,
                        CodigoRecorrido = r.CodigoRecorrido,
                        Ruta = r.Ruta?.Nombre ?? r.Ruta?.CodigoRuta ?? "Ruta no disponible",
                        Conductor = r.Conductor?.NombreCompleto ?? "Conductor no disponible",
                        Bus = r.Bus?.Placa ?? "Bus no disponible",
                        EstadoRecorrido = r.EstadoRecorrido
                    };
                })
                .OrderByDescending(x => x.FechaHora)
                .Take(10)
                .ToList();

            List<AdminMapaParaderoItemViewModel> paraderosMapa = paraderos
                .Where(p => p.Latitud.HasValue && p.Longitud.HasValue)
                .Select(p => new AdminMapaParaderoItemViewModel
                {
                    Latitud = p.Latitud!.Value,
                    Longitud = p.Longitud!.Value,
                    Nombre = p.Nombre,
                    Ruta = p.Ruta?.Nombre ?? p.Ruta?.CodigoRuta ?? "Ruta no disponible"
                })
                .ToList();

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
                TotalNotificacionesRegistradas = notificaciones.Count,
                TotalParaderos = paraderos.Count(x => x.Estado),
                TotalAsignacionesRutaEstudiante = asignaciones.Count(x => x.Estado),
                TotalAbordajesHoy = abordajes.Count(x => x.FechaHora.Date == hoy),
                RecorridosEstadoLabels = recorridos
                    .GroupBy(x => (x.EstadoRecorrido ?? "SIN_ESTADO").Trim().ToUpperInvariant())
                    .Select(g => g.Key)
                    .OrderBy(x => x)
                    .ToList(),
                RecorridosEstadoData = recorridos
                    .GroupBy(x => (x.EstadoRecorrido ?? "SIN_ESTADO").Trim().ToUpperInvariant())
                    .OrderBy(g => g.Key)
                    .Select(g => g.Count())
                    .ToList(),
                IncidenciasEstadoLabels = incidencias
                    .GroupBy(x => (x.EstadoIncidencia ?? "SIN_ESTADO").Trim().ToUpperInvariant())
                    .Select(g => g.Key)
                    .OrderBy(x => x)
                    .ToList(),
                IncidenciasEstadoData = incidencias
                    .GroupBy(x => (x.EstadoIncidencia ?? "SIN_ESTADO").Trim().ToUpperInvariant())
                    .OrderBy(g => g.Key)
                    .Select(g => g.Count())
                    .ToList(),
                IncidenciasPrioridadLabels = incidencias
                    .GroupBy(x => (x.Prioridad ?? "SIN_PRIORIDAD").Trim().ToUpperInvariant())
                    .Select(g => g.Key)
                    .OrderBy(x => x)
                    .ToList(),
                IncidenciasPrioridadData = incidencias
                    .GroupBy(x => (x.Prioridad ?? "SIN_PRIORIDAD").Trim().ToUpperInvariant())
                    .OrderBy(g => g.Key)
                    .Select(g => g.Count())
                    .ToList(),
                RecorridosTendenciaLabels = tendenciaDias.Select(d => d.ToString("dd/MM")).ToList(),
                RecorridosTendenciaData = tendenciaDias.Select(d => recorridos.Count(r => r.Fecha.Date == d.Date)).ToList(),
                IncidenciasTendenciaData = tendenciaDias.Select(d => incidencias.Count(i => i.FechaHora.Date == d.Date)).ToList(),
                BusesMapa = busesMapa,
                ParaderosMapa = paraderosMapa,
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
