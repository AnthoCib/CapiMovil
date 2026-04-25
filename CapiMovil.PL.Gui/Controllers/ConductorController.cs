using CapiMovil.BL.BC;
using CapiMovil.BL.BE;
using CapiMovil.PL.Gui.Infrastructure;
using CapiMovil.PL.Gui.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CapiMovil.PL.Gui.Controllers
{
    public class ConductorController : Controller
    {
        private readonly ConductorBC _conductorBC;
        private readonly PadreFamiliaBC _padreFamiliaBC;
        private readonly UsuarioBC _usuarioBC;
        private readonly RecorridoBC _recorridoBC;
        private readonly IncidenciaBC _incidenciaBC;
        private readonly BusBC _busBC;
        private readonly RutaEstudianteBC _rutaEstudianteBC;
        private readonly EstudianteBC _estudianteBC;
        private readonly EventoAbordajeBC _eventoAbordajeBC;
        private readonly ParaderoBC _paraderoBC;
        private readonly UbicacionBusBC _ubicacionBusBC;

        public ConductorController(
            ConductorBC conductorBC,
            PadreFamiliaBC padreFamiliaBC,
            UsuarioBC usuarioBC,
            RecorridoBC recorridoBC,
            IncidenciaBC incidenciaBC,
            BusBC busBC,
            RutaEstudianteBC rutaEstudianteBC,
            EstudianteBC estudianteBC,
            EventoAbordajeBC eventoAbordajeBC,
            ParaderoBC paraderoBC,
            UbicacionBusBC ubicacionBusBC)
        {
            _conductorBC = conductorBC;
            _padreFamiliaBC = padreFamiliaBC;
            _usuarioBC = usuarioBC;
            _recorridoBC = recorridoBC;
            _incidenciaBC = incidenciaBC;
            _busBC = busBC;
            _rutaEstudianteBC = rutaEstudianteBC;
            _estudianteBC = estudianteBC;
            _eventoAbordajeBC = eventoAbordajeBC;
            _paraderoBC = paraderoBC;
            _ubicacionBusBC = ubicacionBusBC;
        }

        public IActionResult Index()
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Conductor);
            if (acceso != null)
                return acceso;

            ConductorBE? conductor = ObtenerConductorAutenticado();
            if (conductor == null)
            {
                TempData["error"] = "No existe un conductor vinculado al usuario autenticado.";
                return RedirectToAction("SesionInvalida", "Auth");
            }

            List<RecorridoBE> recorridos = _recorridoBC.ListarPorConductor(conductor.IdConductor)
                .OrderByDescending(r => r.Fecha)
                .ThenByDescending(r => PrioridadEstadoRecorrido(r.EstadoRecorrido))
                .ToList();

            RecorridoBE? recorridoHoy = recorridos
                .Where(r => r.Fecha.Date == DateTime.Today)
                .OrderByDescending(r => PrioridadEstadoRecorrido(r.EstadoRecorrido))
                .FirstOrDefault();

            List<IncidenciaBE> incidenciasConductor = _incidenciaBC.ListarPorConductor(conductor.IdConductor);
            int incidenciasAbiertas = incidenciasConductor
                .Count(i => !string.Equals(i.EstadoIncidencia, "CERRADA", StringComparison.OrdinalIgnoreCase));

            List<IncidenciaBE> alertasRecientes = incidenciasConductor
                .OrderByDescending(i => i.FechaHora)
                .Take(4)
                .ToList();

            int estudiantesRutaActiva = 0;
            List<ConductorDashboardEstudianteItemViewModel> estudiantesEnRuta = new();
            if (recorridoHoy != null)
            {
                List<RutaEstudianteBE> asignacionesRuta = _rutaEstudianteBC.Listar()
                    .Where(re => re.IdRuta == recorridoHoy.IdRuta && re.Estado)
                    .ToList();

                estudiantesRutaActiva = asignacionesRuta.Count;

                Dictionary<Guid, string> paraderosSubida = _paraderoBC.ListarPorRuta(recorridoHoy.IdRuta)
                    .ToDictionary(p => p.IdParadero, p => p.Nombre);

                Dictionary<Guid, EstudianteBE> estudiantesDict = _estudianteBC.Listar()
                    .Where(e => asignacionesRuta.Select(a => a.IdEstudiante).Contains(e.IdEstudiante))
                    .ToDictionary(e => e.IdEstudiante, e => e);

                List<EventoAbordajeBE> eventosHoy = _eventoAbordajeBC.Listar()
                    .Where(e => e.IdRecorrido == recorridoHoy.IdRecorrido)
                    .OrderByDescending(e => e.FechaHora)
                    .ToList();

                foreach (RutaEstudianteBE asignacion in asignacionesRuta.Take(8))
                {
                    if (!estudiantesDict.TryGetValue(asignacion.IdEstudiante, out EstudianteBE? estudiante))
                        continue;

                    EventoAbordajeBE? ultimoEvento = eventosHoy.FirstOrDefault(e => e.IdEstudiante == asignacion.IdEstudiante);
                    string estadoEstudiante = ultimoEvento?.TipoEvento switch
                    {
                        "SUBIDA" => "ABORDÓ",
                        "BAJADA" => "DESCENDIÓ",
                        _ => "PENDIENTE"
                    };

                    string hora = ultimoEvento?.FechaHora.ToString("HH:mm") ?? "--:--";
                    string parada = (asignacion.IdParaderoSubida.HasValue && paraderosSubida.TryGetValue(asignacion.IdParaderoSubida.Value, out string? nombreParada))
                        ? nombreParada
                        : "Sin parada";

                    estudiantesEnRuta.Add(new ConductorDashboardEstudianteItemViewModel
                    {
                        Nombre = estudiante.NombreCompleto,
                        Parada = parada,
                        Hora = hora,
                        Estado = estadoEstudiante
                    });
                }
            }

            ConductorDashboardViewModel vm = new()
            {
                NombreConductor = conductor.NombreCompleto,
                RecorridoHoy = recorridoHoy,
                RecorridosHoy = recorridos.Count(r => r.Fecha.Date == DateTime.Today),
                IncidenciasAbiertas = incidenciasAbiertas,
                EstudiantesRutaActiva = estudiantesRutaActiva,
                AlertasRecientes = alertasRecientes,
                EstudiantesEnRuta = estudiantesEnRuta,
                PuedeIniciarRecorrido = recorridoHoy != null && string.Equals(recorridoHoy.EstadoRecorrido, "PROGRAMADO", StringComparison.OrdinalIgnoreCase)
            };

            return View(vm);
        }

        public IActionResult MiConductor()
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Padres);
            if (acceso != null)
                return acceso;

            PadreFamiliaBE? padre = _padreFamiliaBC.ObtenerPorIdUsuario(ObtenerUsuarioIdSesion() ?? Guid.Empty);
            if (padre == null)
            {
                TempData["error"] = "No existe un padre de familia vinculado al usuario autenticado.";
                return RedirectToAction("SesionInvalida", "Auth");
            }

            ConductorBE? conductor = ObtenerConductorParaPadre(padre);

            if (conductor == null)
            {
                TempData["error"] = "No hay un conductor asignado a las rutas de sus hijos en este momento.";
                return RedirectToAction("Index", "PadreFamilia");
            }

            return View(conductor);
        }

        [HttpGet]
        public IActionResult MiBus()
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Conductor);
            if (acceso != null) return acceso;

            ConductorBE? conductor = ObtenerConductorAutenticado();
            if (conductor == null) return RedirectToAction(nameof(Index));

            RecorridoBE? recorridoOperacion = ObtenerRecorridoOperacion(conductor.IdConductor);
            BusBE? bus = recorridoOperacion != null
                ? _busBC.ListarPorId(recorridoOperacion.IdBus)
                : null;

            return View(bus);
        }

        [HttpGet]
        public IActionResult MiRuta()
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Conductor);
            if (acceso != null) return acceso;

            ConductorBE? conductor = ObtenerConductorAutenticado();
            if (conductor == null) return RedirectToAction(nameof(Index));

            List<RecorridoBE> recorridos = _recorridoBC.ListarPorConductor(conductor.IdConductor)
                .OrderByDescending(r => r.Fecha)
                .ToList();

            RecorridoBE? recorridoOperacion = ObtenerRecorridoOperacion(conductor.IdConductor);
            List<ParaderoBE> paraderos = recorridoOperacion == null
                ? new List<ParaderoBE>()
                : _paraderoBC.ListarPorRuta(recorridoOperacion.IdRuta)
                    .OrderBy(p => p.OrdenParada)
                    .ToList();

            List<EstudianteBE> estudiantesRuta = new();
            if (recorridoOperacion != null)
            {
                HashSet<Guid> idsEstudiantes = _rutaEstudianteBC.Listar()
                    .Where(re => re.IdRuta == recorridoOperacion.IdRuta && re.Estado)
                    .Select(re => re.IdEstudiante)
                    .ToHashSet();

                estudiantesRuta = _estudianteBC.Listar()
                    .Where(e => idsEstudiantes.Contains(e.IdEstudiante))
                    .OrderBy(e => e.ApellidoPaterno)
                    .ThenBy(e => e.Nombres)
                    .ToList();
            }

            UbicacionBusBE? ultimaUbicacion = recorridoOperacion == null
                ? null
                : _ubicacionBusBC.Listar()
                    .Where(u => u.IdRecorrido == recorridoOperacion.IdRecorrido && u.Estado)
                    .OrderByDescending(u => u.FechaHora)
                    .FirstOrDefault();

            ConductorMiRutaViewModel vm = new()
            {
                RecorridoOperacion = recorridoOperacion,
                Recorridos = recorridos,
                Paraderos = paraderos,
                EstudiantesRuta = estudiantesRuta,
                UltimaUbicacionBus = ultimaUbicacion
            };

            return View(vm);
        }

        [HttpGet]
        public IActionResult Estudiantes()
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Conductor);
            if (acceso != null) return acceso;

            ConductorBE? conductor = ObtenerConductorAutenticado();
            if (conductor == null) return RedirectToAction(nameof(Index));

            HashSet<Guid> rutasDelConductor = _recorridoBC.ListarPorConductor(conductor.IdConductor)
                .Select(r => r.IdRuta)
                .ToHashSet();

            HashSet<Guid> idsEstudiantes = _rutaEstudianteBC.Listar()
                .Where(re => rutasDelConductor.Contains(re.IdRuta) && re.Estado)
                .Select(re => re.IdEstudiante)
                .ToHashSet();

            List<EstudianteBE> estudiantes = _estudianteBC.Listar()
                .Where(e => idsEstudiantes.Contains(e.IdEstudiante))
                .OrderBy(e => e.ApellidoPaterno)
                .ThenBy(e => e.Nombres)
                .ToList();

            return View(estudiantes);
        }

        [HttpGet]
        public IActionResult Recorrido()
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Conductor);
            if (acceso != null) return acceso;

            ConductorBE? conductor = ObtenerConductorAutenticado();
            if (conductor == null) return RedirectToAction(nameof(Index));

            List<RecorridoBE> recorridos = _recorridoBC.ListarPorConductor(conductor.IdConductor)
                .OrderByDescending(r => r.Fecha)
                .ToList();

            return View(recorridos);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult IniciarRecorrido(Guid idRecorrido)
        {
            return EjecutarCambioRecorrido(
                idRecorrido,
                id => _recorridoBC.Iniciar(id, ObtenerUsuarioIdSesion(), ObtenerUsernameSesion(), ObtenerIpCliente(), ObtenerUserAgent()),
                "Recorrido iniciado correctamente.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult FinalizarRecorrido(Guid idRecorrido)
        {
            return EjecutarCambioRecorrido(
                idRecorrido,
                id => _recorridoBC.Finalizar(id, ObtenerUsuarioIdSesion(), ObtenerUsernameSesion(), ObtenerIpCliente(), ObtenerUserAgent()),
                "Recorrido finalizado correctamente.");
        }

        [HttpGet]
        public IActionResult Abordaje()
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Conductor);
            if (acceso != null) return acceso;

            ConductorBE? conductor = ObtenerConductorAutenticado();
            if (conductor == null) return RedirectToAction(nameof(Index));

            ConductorAbordajeViewModel vm = ConstruirVistaAbordaje(conductor);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RegistrarAbordaje(ConductorAbordajeViewModel vm)
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Conductor);
            if (acceso != null) return acceso;

            ConductorBE? conductor = ObtenerConductorAutenticado();
            if (conductor == null) return RedirectToAction(nameof(Index));

            RecorridoBE? recorridoActivo = _recorridoBC.ObtenerActivoPorConductor(conductor.IdConductor);
            if (recorridoActivo == null)
            {
                TempData["error"] = "No existe un recorrido activo para registrar abordajes.";
                return RedirectToAction(nameof(Abordaje));
            }

            if (vm.IdEstudiante == Guid.Empty)
            {
                TempData["error"] = "Debe seleccionar un estudiante.";
                return RedirectToAction(nameof(Abordaje));
            }

            string tipoEvento = (vm.TipoEvento ?? string.Empty).Trim().ToUpperInvariant();
            HashSet<string> tiposPermitidos = new(StringComparer.OrdinalIgnoreCase)
            {
                "SUBIDA",
                "BAJADA",
                "AUSENTE",
                "NO_ABORDO"
            };

            if (!tiposPermitidos.Contains(tipoEvento))
            {
                TempData["error"] = "El tipo de evento no es válido.";
                return RedirectToAction(nameof(Abordaje));
            }

            HashSet<Guid> estudiantesPermitidos = _rutaEstudianteBC.Listar()
                .Where(re => re.IdRuta == recorridoActivo.IdRuta && re.Estado)
                .Select(re => re.IdEstudiante)
                .ToHashSet();

            if (!estudiantesPermitidos.Contains(vm.IdEstudiante))
            {
                TempData["error"] = "El alumno no pertenece al recorrido activo.";
                return RedirectToAction(nameof(Abordaje));
            }

            try
            {
                string? usuarioIdSession = HttpContext.Session.GetString("UsuarioId");
                Guid? usuarioId = string.IsNullOrWhiteSpace(usuarioIdSession) ? null : Guid.Parse(usuarioIdSession);

                EventoAbordajeBE entidad = new()
                {
                    IdRecorrido = recorridoActivo.IdRecorrido,
                    IdEstudiante = vm.IdEstudiante,
                    IdParadero = vm.IdParadero,
                    RegistradoPor = usuarioId,
                    TipoEvento = tipoEvento,
                    FechaHora = vm.FechaHora == default ? DateTime.Now : vm.FechaHora,
                    Observacion = vm.Observacion,
                    Estado = true
                };

                _eventoAbordajeBC.ValidarRegistro(entidad);
                bool ok = _eventoAbordajeBC.Registrar(entidad);
                TempData[ok ? "ok" : "error"] = ok
                    ? $"Evento registrado correctamente. Código: {entidad.CodigoEvento}"
                    : "No se pudo registrar el evento de abordaje.";
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }

            return RedirectToAction(nameof(Abordaje));
        }

        [HttpGet]
        public IActionResult Incidencias()
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Conductor);
            if (acceso != null) return acceso;

            ConductorBE? conductor = ObtenerConductorAutenticado();
            if (conductor == null) return RedirectToAction(nameof(Index));

            ConductorIncidenciasViewModel vm = ConstruirVistaIncidencias(conductor);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RegistrarIncidencia(ConductorIncidenciasViewModel vm)
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Conductor);
            if (acceso != null) return acceso;

            ConductorBE? conductor = ObtenerConductorAutenticado();
            if (conductor == null) return RedirectToAction(nameof(Index));

            RecorridoBE? recorridoOperacion = ObtenerRecorridoOperacion(conductor.IdConductor);
            if (recorridoOperacion == null)
            {
                TempData["error"] = "No existe recorrido asignado para registrar incidencias.";
                return RedirectToAction(nameof(Incidencias));
            }

            if (string.IsNullOrWhiteSpace(vm.Descripcion))
            {
                TempData["error"] = "Debe ingresar la descripción de la incidencia.";
                return RedirectToAction(nameof(Incidencias));
            }

            try
            {
                string? usuarioIdSession = HttpContext.Session.GetString("UsuarioId");
                Guid? usuarioId = string.IsNullOrWhiteSpace(usuarioIdSession) ? null : Guid.Parse(usuarioIdSession);

                IncidenciaBE entidad = new()
                {
                    IdRecorrido = recorridoOperacion.IdRecorrido,
                    IdConductor = conductor.IdConductor,
                    ReportadoPor = usuarioId,
                    TipoIncidencia = vm.TipoIncidencia,
                    Descripcion = vm.Descripcion,
                    FechaHora = vm.FechaHora,
                    EstadoIncidencia = "PENDIENTE",
                    Prioridad = vm.Prioridad
                };

                bool ok = _incidenciaBC.Registrar(entidad);
                TempData[ok ? "ok" : "error"] = ok
                    ? "Incidencia registrada correctamente."
                    : "No se pudo registrar la incidencia.";
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }

            return RedirectToAction(nameof(Incidencias));
        }

        [HttpGet]
        public IActionResult Listar()
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Administracion);
            if (acceso != null) return acceso;

            var lista = _conductorBC.Listar();
            return View(lista);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Administracion);
            if (acceso != null) return acceso;

            ConductorFormViewModel vm = new()
            {
                Estado = true,
                Usuarios = ObtenerUsuariosDisponibles()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(ConductorFormViewModel vm)
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Administracion);
            if (acceso != null) return acceso;

            if (vm.IdUsuario == Guid.Empty)
                ModelState.AddModelError(nameof(vm.IdUsuario), "Debe seleccionar un usuario.");

            if (!ModelState.IsValid)
            {
                vm.Usuarios = ObtenerUsuariosDisponibles();
                return View(vm);
            }

            try
            {
                ConductorBE entidad = new()
                {
                    IdUsuario = vm.IdUsuario,
                    Nombres = vm.Nombres,
                    ApellidoPaterno = vm.ApellidoPaterno,
                    ApellidoMaterno = vm.ApellidoMaterno,
                    DNI = vm.DNI,
                    Licencia = vm.Licencia,
                    CategoriaLicencia = vm.CategoriaLicencia,
                    Telefono = vm.Telefono,
                    Direccion = vm.Direccion,
                    Estado = vm.Estado
                };

                bool ok = _conductorBC.Registrar(entidad);

                if (ok)
                {
                    TempData["ok"] = "Conductor registrado correctamente.";
                    return RedirectToAction(nameof(Listar));
                }

                const string mensajeError = "No se pudo registrar el conductor.";
                ModelState.AddModelError(string.Empty, mensajeError);
                ViewBag.SwalError = mensajeError;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewBag.SwalError = ex.Message;
            }

            vm.Usuarios = ObtenerUsuariosDisponibles();
            return View(vm);
        }

        [HttpGet]
        public IActionResult Editar(Guid id)
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Administracion);
            if (acceso != null) return acceso;

            var entidad = _conductorBC.ListarPorId(id);

            if (entidad == null)
            {
                TempData["error"] = "Conductor no encontrado.";
                return RedirectToAction(nameof(Listar));
            }

            ConductorFormViewModel vm = new()
            {
                IdConductor = entidad.IdConductor,
                IdUsuario = entidad.IdUsuario,
                CodigoConductor = entidad.CodigoConductor,
                Nombres = entidad.Nombres,
                ApellidoPaterno = entidad.ApellidoPaterno,
                ApellidoMaterno = entidad.ApellidoMaterno,
                DNI = entidad.DNI,
                Licencia = entidad.Licencia,
                CategoriaLicencia = entidad.CategoriaLicencia,
                Telefono = entidad.Telefono,
                Direccion = entidad.Direccion,
                Estado = entidad.Estado,
                Usuarios = ObtenerUsuariosParaEdicion(entidad.IdUsuario)
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(ConductorFormViewModel vm)
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Administracion);
            if (acceso != null) return acceso;

            if (vm.IdUsuario == Guid.Empty)
                ModelState.AddModelError(nameof(vm.IdUsuario), "Debe seleccionar un usuario.");

            if (!ModelState.IsValid)
            {
                vm.Usuarios = ObtenerUsuariosParaEdicion(vm.IdUsuario);
                return View(vm);
            }

            try
            {
                ConductorBE entidad = new()
                {
                    IdConductor = vm.IdConductor,
                    IdUsuario = vm.IdUsuario,
                    CodigoConductor = vm.CodigoConductor,
                    Nombres = vm.Nombres,
                    ApellidoPaterno = vm.ApellidoPaterno,
                    ApellidoMaterno = vm.ApellidoMaterno,
                    DNI = vm.DNI,
                    Licencia = vm.Licencia,
                    CategoriaLicencia = vm.CategoriaLicencia,
                    Telefono = vm.Telefono,
                    Direccion = vm.Direccion,
                    Estado = vm.Estado
                };

                bool ok = _conductorBC.Actualizar(entidad);

                TempData[ok ? "ok" : "error"] = ok
                    ? "Conductor actualizado correctamente."
                    : "No se pudo actualizar el conductor.";

                if (ok)
                    return RedirectToAction(nameof(Listar));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewBag.SwalError = ex.Message;
            }

            vm.Usuarios = ObtenerUsuariosParaEdicion(vm.IdUsuario);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(Guid id)
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Administracion);
            if (acceso != null) return acceso;

            try
            {
                bool ok = _conductorBC.Eliminar(id);

                TempData[ok ? "ok" : "error"] = ok
                    ? "Conductor eliminado correctamente."
                    : "No se pudo eliminar el conductor.";
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }

            return RedirectToAction(nameof(Listar));
        }

        private IActionResult EjecutarCambioRecorrido(Guid idRecorrido, Func<Guid, bool> operacion, string mensajeExito)
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Conductor);
            if (acceso != null) return acceso;

            ConductorBE? conductor = ObtenerConductorAutenticado();
            if (conductor == null) return RedirectToAction(nameof(Index));

            RecorridoBE? recorrido = _recorridoBC.ListarPorId(idRecorrido);
            if (recorrido == null || recorrido.IdConductor != conductor.IdConductor)
            {
                TempData["error"] = "No tiene permisos para operar este recorrido.";
                return RedirectToAction(nameof(Recorrido));
            }

            try
            {
                bool ok = operacion(idRecorrido);
                TempData[ok ? "ok" : "error"] = ok ? mensajeExito : "No se pudo ejecutar la operación.";
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }

            return RedirectToAction(nameof(Recorrido));
        }

        private ConductorAbordajeViewModel ConstruirVistaAbordaje(ConductorBE conductor)
        {
            RecorridoBE? recorridoActivo = _recorridoBC.ObtenerActivoPorConductor(conductor.IdConductor);
            HashSet<Guid> idsRecorrido = _recorridoBC.ListarPorConductor(conductor.IdConductor)
                .Select(r => r.IdRecorrido)
                .ToHashSet();

            List<EventoAbordajeBE> eventos = _eventoAbordajeBC.Listar()
                .Where(e => idsRecorrido.Contains(e.IdRecorrido))
                .OrderByDescending(e => e.FechaHora)
                .ToList();

            List<SelectListItem> estudiantes = new();
            List<SelectListItem> paraderos = new();

            if (recorridoActivo != null)
            {
                HashSet<Guid> idsEstudiantes = _rutaEstudianteBC.Listar()
                    .Where(re => re.IdRuta == recorridoActivo.IdRuta && re.Estado)
                    .Select(re => re.IdEstudiante)
                    .ToHashSet();

                estudiantes = _estudianteBC.Listar()
                    .Where(e => idsEstudiantes.Contains(e.IdEstudiante))
                    .OrderBy(e => e.ApellidoPaterno)
                    .ThenBy(e => e.Nombres)
                    .Select(e => new SelectListItem
                    {
                        Value = e.IdEstudiante.ToString(),
                        Text = $"{e.CodigoEstudiante} - {e.NombreCompleto}"
                    })
                    .ToList();

                paraderos = _paraderoBC.ListarPorRuta(recorridoActivo.IdRuta)
                    .OrderBy(p => p.OrdenParada)
                    .Select(p => new SelectListItem
                    {
                        Value = p.IdParadero.ToString(),
                        Text = $"{p.OrdenParada}. {p.Nombre}"
                    })
                    .ToList();
            }

            List<ConductorAbordajeAlumnoEstadoViewModel> estadosPorAlumno = new();
            if (recorridoActivo != null)
            {
                foreach (SelectListItem estudiante in estudiantes)
                {
                    if (!Guid.TryParse(estudiante.Value, out Guid idEstudiante))
                        continue;

                    var resumen = _eventoAbordajeBC.ObtenerResumenPorEstudianteRecorrido(recorridoActivo.IdRecorrido, idEstudiante);
                    estadosPorAlumno.Add(MapearEstadoAbordaje(estudiante, resumen));
                }
            }

            return new ConductorAbordajeViewModel
            {
                RecorridoActivo = recorridoActivo,
                Eventos = eventos,
                Estudiantes = estudiantes,
                Paraderos = paraderos,
                TiposEvento = new List<SelectListItem>(),
                EstadosPorAlumno = estadosPorAlumno
            };
        }

        private static ConductorAbordajeAlumnoEstadoViewModel MapearEstadoAbordaje(SelectListItem estudiante, EventoAbordajeResumenBE resumen)
        {
            bool tieneSubida = resumen.TotalSubidas > 0;
            bool tieneBajada = resumen.TotalBajadas > 0;
            bool ausente = resumen.TotalAusentes > 0;
            bool noAbordo = resumen.TotalNoAbordo > 0;

            string estadoActual = "SIN_EVENTOS";
            if (ausente) estadoActual = "AUSENTE";
            else if (noAbordo) estadoActual = "NO_ABORDO";
            else if (tieneSubida && tieneBajada) estadoActual = "COMPLETADO";
            else if (tieneSubida) estadoActual = "EN_BUS";
            else estadoActual = "PENDIENTE";

            bool bloqueado = ausente || noAbordo;

            return new ConductorAbordajeAlumnoEstadoViewModel
            {
                IdEstudiante = Guid.Parse(estudiante.Value!),
                NombreEstudiante = estudiante.Text,
                TotalSubidas = resumen.TotalSubidas,
                TotalBajadas = resumen.TotalBajadas,
                TotalAusentes = resumen.TotalAusentes,
                TotalNoAbordo = resumen.TotalNoAbordo,
                EstadoActual = bloqueado ? "BLOQUEADO" : estadoActual,
                PermiteSubida = !ausente && !noAbordo && !(tieneSubida && !tieneBajada) && !(tieneSubida && tieneBajada),
                PermiteBajada = !ausente && !noAbordo && tieneSubida && !tieneBajada,
                PermiteAusente = !ausente && !noAbordo && !tieneSubida && !tieneBajada,
                PermiteNoAbordo = !ausente && !noAbordo && !tieneSubida && !tieneBajada
            };
        }

        private ConductorIncidenciasViewModel ConstruirVistaIncidencias(ConductorBE conductor)
        {
            RecorridoBE? recorridoOperacion = ObtenerRecorridoOperacion(conductor.IdConductor);

            return new ConductorIncidenciasViewModel
            {
                RecorridoOperacion = recorridoOperacion,
                Incidencias = _incidenciaBC.ListarPorConductor(conductor.IdConductor)
                    .OrderByDescending(i => i.FechaHora)
                    .ToList(),
                TiposIncidencia = new List<SelectListItem>
                {
                    new("RETRASO", "RETRASO"),
                    new("FALLA MECANICA", "FALLA MECANICA"),
                    new("ACCIDENTE", "ACCIDENTE"),
                    new("DESVIO", "DESVIO"),
                    new("AUSENCIA", "AUSENCIA"),
                    new("OTRO", "OTRO")
                },
                Prioridades = new List<SelectListItem>
                {
                    new("BAJA", "BAJA"),
                    new("MEDIA", "MEDIA"),
                    new("ALTA", "ALTA"),
                    new("CRITICA", "CRITICA")
                }
            };
        }

        private RecorridoBE? ObtenerRecorridoOperacion(Guid idConductor)
        {
            RecorridoBE? recorridoActivo = _recorridoBC.ObtenerActivoPorConductor(idConductor);
            if (recorridoActivo != null)
                return recorridoActivo;

            return _recorridoBC.ListarPorConductor(idConductor)
                .Where(r => r.Fecha.Date == DateTime.Today)
                .OrderByDescending(r => PrioridadEstadoRecorrido(r.EstadoRecorrido))
                .FirstOrDefault();
        }

        private static int PrioridadEstadoRecorrido(string? estado)
        {
            string normalizado = (estado ?? string.Empty).Trim().ToUpperInvariant();
            return normalizado switch
            {
                "EN_CURSO" => 3,
                "PROGRAMADO" => 2,
                "FINALIZADO" => 1,
                _ => 0
            };
        }

        private ConductorBE? ObtenerConductorAutenticado()
        {
            string? usuarioId = HttpContext.Session.GetString("UsuarioId");
            if (!Guid.TryParse(usuarioId, out Guid idUsuario))
                return null;

            return _conductorBC.ObtenerPorIdUsuario(idUsuario);
        }

        private Guid? ObtenerUsuarioIdSesion()
        {
            string? usuarioId = HttpContext.Session.GetString("UsuarioId");
            return Guid.TryParse(usuarioId, out Guid idUsuario) ? idUsuario : null;
        }

        private string? ObtenerIpCliente()
            => HttpContext.Connection.RemoteIpAddress?.ToString();

        private string? ObtenerUserAgent()
            => Request.Headers.UserAgent.ToString();

        private string? ObtenerUsernameSesion()
        {
            string? username = HttpContext.Session.GetString("Username");
            if (!string.IsNullOrWhiteSpace(username))
                return username;

            Guid? idUsuario = ObtenerUsuarioIdSesion();
            if (idUsuario.HasValue)
                return _usuarioBC.ListarPorId(idUsuario.Value)?.Username;

            return null;
        }

        private ConductorBE? ObtenerConductorParaPadre(PadreFamiliaBE padre)
        {
            HashSet<Guid> idsHijos = _estudianteBC.Listar()
                .Where(e => e.IdPadre == padre.IdPadre)
                .Select(e => e.IdEstudiante)
                .ToHashSet();

            if (idsHijos.Count == 0)
                return null;

            HashSet<Guid> idsRuta = _rutaEstudianteBC.Listar()
                .Where(re => re.Estado && idsHijos.Contains(re.IdEstudiante))
                .Select(re => re.IdRuta)
                .ToHashSet();

            if (idsRuta.Count == 0)
                return null;

            RecorridoBE? recorrido = _recorridoBC.Listar()
                .Where(r => idsRuta.Contains(r.IdRuta))
                .OrderByDescending(r => r.Fecha.Date == DateTime.Today)
                .ThenByDescending(r => PrioridadEstadoRecorrido(r.EstadoRecorrido))
                .ThenByDescending(r => r.Fecha)
                .FirstOrDefault();

            return recorrido == null ? null : _conductorBC.ListarPorId(recorrido.IdConductor);
        }

        private List<SelectListItem> ObtenerUsuariosDisponibles()
        {
            return _conductorBC.ListarUsuariosDisponibles()
                .Select(u => new SelectListItem
                {
                    Value = u.IdUsuario.ToString(),
                    Text = $"{u.Username} - {u.Correo}"
                })
                .ToList();
        }

        private List<SelectListItem> ObtenerUsuariosParaEdicion(Guid idUsuarioActual)
        {
            var disponibles = _conductorBC.ListarUsuariosDisponibles();
            var usuarioActual = _usuarioBC.ListarPorId(idUsuarioActual);

            if (usuarioActual != null && disponibles.All(x => x.IdUsuario != idUsuarioActual))
                disponibles.Insert(0, usuarioActual);

            return disponibles
                .Select(u => new SelectListItem
                {
                    Value = u.IdUsuario.ToString(),
                    Text = $"{u.Username} - {u.Correo}",
                    Selected = u.IdUsuario == idUsuarioActual
                })
                .ToList();
        }
    }
}
