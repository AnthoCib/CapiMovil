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

        [HttpGet]
        public IActionResult Index(Guid? idRecorridoOperacion, Guid? idParaderoActual, string? tipoParadero)
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

            ConductorDashboardViewModel vm = ConstruirDashboardConductor(conductor);

            if (idRecorridoOperacion.HasValue &&
                idParaderoActual.HasValue &&
                !string.IsNullOrWhiteSpace(tipoParadero))
            {
                CargarOperacionParadero(
                    vm,
                    idRecorridoOperacion.Value,
                    idParaderoActual.Value,
                    tipoParadero
                );
            }

            return View(vm);
        }

        [HttpGet]
        public IActionResult MiBus()
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Conductor);
            if (acceso != null)
                return acceso;

            ConductorBE? conductor = ObtenerConductorAutenticado();

            if (conductor == null)
                return RedirectToAction(nameof(Index));

            ConductorDashboardViewModel vm = ConstruirDashboardConductor(conductor);
            return View(vm);
        }

        [HttpGet]
        public IActionResult MiRuta()
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Conductor);
            if (acceso != null)
                return acceso;

            ConductorBE? conductor = ObtenerConductorAutenticado();

            if (conductor == null)
                return RedirectToAction(nameof(Index));

            ConductorDashboardViewModel vm = ConstruirDashboardConductor(conductor);
            return View(vm);
        }

        [HttpGet]
        public IActionResult Estudiantes()
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Conductor);
            if (acceso != null)
                return acceso;

            ConductorBE? conductor = ObtenerConductorAutenticado();

            if (conductor == null)
                return RedirectToAction(nameof(Index));

            ConductorDashboardViewModel vm = ConstruirDashboardConductor(conductor);
            return View(vm);
        }

        [HttpGet]
        public IActionResult Recorrido()
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Conductor);
            if (acceso != null)
                return acceso;

            ConductorBE? conductor = ObtenerConductorAutenticado();

            if (conductor == null)
                return RedirectToAction(nameof(Index));

            ConductorDashboardViewModel vm = ConstruirDashboardConductor(conductor);
            return View(vm);
        }

        [HttpGet]
        public IActionResult Incidencias()
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Conductor);
            if (acceso != null)
                return acceso;

            ConductorBE? conductor = ObtenerConductorAutenticado();

            if (conductor == null)
                return RedirectToAction(nameof(Index));

            ConductorDashboardViewModel vm = ConstruirDashboardConductor(conductor);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult IniciarRecorrido(Guid idRecorrido)
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Conductor);
            if (acceso != null)
                return acceso;

            ConductorBE? conductor = ObtenerConductorAutenticado();

            if (conductor == null)
                return RedirectToAction(nameof(Index));

            RecorridoBE? recorrido = _recorridoBC.ListarPorId(idRecorrido);

            if (recorrido == null || recorrido.IdConductor != conductor.IdConductor)
            {
                TempData["error"] = "No tiene permisos para iniciar este recorrido.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                bool puedeIniciar = _recorridoBC.PuedeIniciarSegunOrden(idRecorrido);

                if (!puedeIniciar)
                {
                    TempData["error"] = "No puedes iniciar este recorrido todavía. Primero debes finalizar el recorrido anterior.";
                    return RedirectToAction(nameof(Index));
                }

                bool ok = _recorridoBC.Iniciar(
                    idRecorrido,
                    ObtenerUsuarioIdSesion(),
                    ObtenerUsernameSesion(),
                    ObtenerIpCliente(),
                    ObtenerUserAgent()
                );

                TempData[ok ? "ok" : "error"] = ok
                    ? "Recorrido iniciado correctamente."
                    : "No se pudo iniciar el recorrido.";
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult FinalizarRecorrido(Guid idRecorrido)
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Conductor);
            if (acceso != null)
                return acceso;

            ConductorBE? conductor = ObtenerConductorAutenticado();

            if (conductor == null)
                return RedirectToAction(nameof(Index));

            RecorridoBE? recorrido = _recorridoBC.ListarPorId(idRecorrido);

            if (recorrido == null || recorrido.IdConductor != conductor.IdConductor)
            {
                TempData["error"] = "No tiene permisos para finalizar este recorrido.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                bool ok = _recorridoBC.Finalizar(
                    idRecorrido,
                    ObtenerUsuarioIdSesion(),
                    ObtenerUsernameSesion(),
                    ObtenerIpCliente(),
                    ObtenerUserAgent()
                );

                TempData[ok ? "ok" : "error"] = ok
                    ? "Recorrido finalizado correctamente."
                    : "No se pudo finalizar el recorrido.";
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GuardarEventosParadero(
            Guid idRecorrido,
            Guid idParadero,
            string tipoParadero,
            List<Guid> idsEstudiantes,
            IFormCollection form)
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Conductor);
            if (acceso != null)
                return acceso;

            ConductorBE? conductor = ObtenerConductorAutenticado();

            if (conductor == null)
                return RedirectToAction(nameof(Index));

            RecorridoBE? recorrido = _recorridoBC.ListarPorId(idRecorrido);

            if (recorrido == null || recorrido.IdConductor != conductor.IdConductor)
            {
                TempData["error"] = "No tiene permisos para registrar eventos en este recorrido.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                Guid? usuarioSesion = ObtenerUsuarioIdSesion();

                if (!usuarioSesion.HasValue)
                {
                    TempData["error"] = "No se pudo obtener el usuario de sesión.";
                    return RedirectToAction(nameof(Index));
                }

                if (idsEstudiantes == null || !idsEstudiantes.Any())
                {
                    TempData["error"] = "No hay estudiantes para registrar.";
                    return RedirectToAction(nameof(Index));
                }

                string tipoParaderoNormalizado = (tipoParadero ?? "").Trim().ToUpperInvariant();

                bool datosValidos = ValidarEventosParadero(
                    idRecorrido,
                    idParadero,
                    tipoParaderoNormalizado,
                    idsEstudiantes,
                    form
                );

                if (!datosValidos)
                {
                    return RedirectToAction(nameof(Index), new
                    {
                        idRecorridoOperacion = idRecorrido,
                        idParaderoActual = idParadero,
                        tipoParadero = tipoParaderoNormalizado
                    });
                }

                foreach (Guid idEstudiante in idsEstudiantes)
                {
                    string tipoEvento = form["tipoEvento_" + idEstudiante]
                        .ToString()
                        .Trim()
                        .ToUpperInvariant();

                    _recorridoBC.RegistrarEventoAbordajeConductor(
                        idRecorrido,
                        idEstudiante,
                        idParadero,
                        usuarioSesion.Value,
                        tipoEvento,
                        null
                    );
                }

                if (tipoParaderoNormalizado == "SUBIDA")
                {
                    TempData["ok"] = "Paradero de recojo registrado correctamente. Selecciona manualmente el siguiente paradero.";
                }
                else if (tipoParaderoNormalizado == "BAJADA")
                {
                    TempData["ok"] = "Paradero de entrega registrado correctamente. Selecciona manualmente el siguiente paradero.";
                }
                else
                {
                    TempData["ok"] = "Eventos registrados correctamente.";
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RegistrarIncidenciaConductor(
            Guid idRecorrido,
            string tipoIncidencia,
            string prioridad,
            string descripcion)
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Conductor);
            if (acceso != null)
                return acceso;

            ConductorBE? conductor = ObtenerConductorAutenticado();

            if (conductor == null)
                return RedirectToAction(nameof(Index));

            RecorridoBE? recorrido = _recorridoBC.ListarPorId(idRecorrido);

            if (recorrido == null || recorrido.IdConductor != conductor.IdConductor)
            {
                TempData["error"] = "No tiene permisos para registrar incidencia en este recorrido.";
                return RedirectToAction(nameof(Incidencias));
            }

            if (string.IsNullOrWhiteSpace(descripcion))
            {
                TempData["error"] = "Debe ingresar una descripción.";
                return RedirectToAction(nameof(Incidencias));
            }

            try
            {
                Guid? usuarioId = ObtenerUsuarioIdSesion();

                IncidenciaBE incidencia = new()
                {
                    IdRecorrido = idRecorrido,
                    IdConductor = conductor.IdConductor,
                    ReportadoPor = usuarioId,
                    TipoIncidencia = (tipoIncidencia ?? "").Trim().ToUpperInvariant(),
                    Descripcion = descripcion.Trim(),
                    FechaHora = DateTime.Now,
                    EstadoIncidencia = "PENDIENTE",
                    Prioridad = string.IsNullOrWhiteSpace(prioridad)
                        ? "MEDIA"
                        : prioridad.Trim().ToUpperInvariant(),
                    Solucion = null,
                    Estado = true
                };

                bool ok = _incidenciaBC.Registrar(incidencia);

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
        public IActionResult Abordaje()
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Conductor);
            if (acceso != null)
                return acceso;

            ConductorBE? conductor = ObtenerConductorAutenticado();

            if (conductor == null)
                return RedirectToAction(nameof(Index));

            ConductorAbordajeViewModel vm = ConstruirVistaAbordaje(conductor);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RegistrarAbordaje(ConductorAbordajeViewModel vm)
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Conductor);
            if (acceso != null)
                return acceso;

            ConductorBE? conductor = ObtenerConductorAutenticado();

            if (conductor == null)
                return RedirectToAction(nameof(Index));

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
                Guid? usuarioId = ObtenerUsuarioIdSesion();

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RegistrarIncidencia(ConductorIncidenciasViewModel vm)
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Conductor);
            if (acceso != null)
                return acceso;

            ConductorBE? conductor = ObtenerConductorAutenticado();

            if (conductor == null)
                return RedirectToAction(nameof(Index));

            RecorridoBE? recorridoOperacion = ObtenerRecorridoOperacion(conductor.IdConductor);

            if (recorridoOperacion == null)
            {
                TempData["error"] = "No existe un recorrido para registrar incidencias.";
                return RedirectToAction(nameof(Incidencias));
            }

            if (string.IsNullOrWhiteSpace(vm.Descripcion))
            {
                TempData["error"] = "Debe ingresar la descripción de la incidencia.";
                return RedirectToAction(nameof(Incidencias));
            }

            try
            {
                Guid? usuarioId = ObtenerUsuarioIdSesion();

                IncidenciaBE entidad = new()
                {
                    IdRecorrido = recorridoOperacion.IdRecorrido,
                    IdConductor = conductor.IdConductor,
                    ReportadoPor = usuarioId,
                    TipoIncidencia = vm.TipoIncidencia,
                    Descripcion = vm.Descripcion,
                    FechaHora = vm.FechaHora == default ? DateTime.Now : vm.FechaHora,
                    EstadoIncidencia = "PENDIENTE",
                    Prioridad = vm.Prioridad,
                    Estado = true
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
            if (acceso != null)
                return acceso;

            var lista = _conductorBC.Listar();
            return View(lista);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Administracion);
            if (acceso != null)
                return acceso;

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
            if (acceso != null)
                return acceso;

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
            if (acceso != null)
                return acceso;

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
            if (acceso != null)
                return acceso;

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
            if (acceso != null)
                return acceso;

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

        private ConductorDashboardViewModel ConstruirDashboardConductor(ConductorBE conductor)
        {
            ConductorDashboardViewModel vm = new()
            {
                NombreConductor = conductor.NombreCompleto
            };

            List<RecorridoBE> recorridosHoy = _recorridoBC.ListarHoyPorConductor(
                conductor.IdConductor,
                DateTime.Today
            );

            vm.RecorridoActual = recorridosHoy.FirstOrDefault();

            vm.RecorridosHoy = recorridosHoy
                .Select(r => CrearResumenRecorrido(r))
                .ToList();

            if (vm.RecorridoActual != null)
            {
                CargarDatosRecorrido(vm, vm.RecorridoActual.IdRecorrido);
            }

            return vm;
        }

        private void CargarDatosRecorrido(ConductorDashboardViewModel vm, Guid idRecorrido)
        {
            vm.EstudiantesEnRuta = _recorridoBC.ListarEstudiantesPorRecorridoConductor(idRecorrido);

            vm.ParaderosRecojo = _recorridoBC.ListarParaderosPorRecorrido(idRecorrido, "SUBIDA")
                .Select(p => CrearResumenParadero(p))
                .ToList();

            vm.ParaderosEntrega = _recorridoBC.ListarParaderosPorRecorrido(idRecorrido, "BAJADA")
                .Select(p => CrearResumenParadero(p))
                .ToList();

            vm.IncidenciasPendientes = _recorridoBC.ListarIncidenciasPendientesConductor(idRecorrido)
                .Select(i => new IncidenciaResumenConductorViewModel
                {
                    TipoIncidencia = i.TipoIncidencia,
                    Descripcion = i.Descripcion,
                    Prioridad = i.Prioridad,
                    FechaHora = i.FechaHora
                })
                .ToList();
        }

        private void CargarOperacionParadero(
            ConductorDashboardViewModel vm,
            Guid idRecorridoOperacion,
            Guid idParaderoActual,
            string? tipoParadero)
        {
            string tipo = (tipoParadero ?? "").Trim().ToUpperInvariant();

            if (tipo != "SUBIDA" && tipo != "BAJADA")
                return;

            vm.IdRecorridoOperacion = idRecorridoOperacion;
            vm.IdParaderoActual = idParaderoActual;
            vm.TipoParaderoActual = tipo;

            ParaderoResumenConductorViewModel? paraderoSeleccionado = vm.ParaderosRecojo
                .FirstOrDefault(p => p.IdParadero == idParaderoActual);

            if (paraderoSeleccionado == null)
            {
                paraderoSeleccionado = vm.ParaderosEntrega
                    .FirstOrDefault(p => p.IdParadero == idParaderoActual);
            }

            if (paraderoSeleccionado != null)
            {
                vm.NombreParaderoActual = paraderoSeleccionado.Nombre;
            }

            vm.EstudiantesParaderoActual = _recorridoBC.ListarEstudiantesPorParaderoConductor(
                idRecorridoOperacion,
                idParaderoActual,
                tipo
            );
        }

        private bool ValidarEventosParadero(
            Guid idRecorrido,
            Guid idParadero,
            string tipoParaderoNormalizado,
            List<Guid> idsEstudiantes,
            IFormCollection form)
        {
            if (idRecorrido == Guid.Empty)
            {
                TempData["error"] = "Id de recorrido inválido.";
                return false;
            }

            if (idParadero == Guid.Empty)
            {
                TempData["error"] = "Id de paradero inválido.";
                return false;
            }

            if (tipoParaderoNormalizado != "SUBIDA" && tipoParaderoNormalizado != "BAJADA")
            {
                TempData["error"] = "Tipo de paradero inválido.";
                return false;
            }

            foreach (Guid idEstudiante in idsEstudiantes)
            {
                string tipoEvento = form["tipoEvento_" + idEstudiante].ToString();

                if (string.IsNullOrWhiteSpace(tipoEvento))
                {
                    TempData["error"] = "Debe seleccionar el estado de todos los alumnos.";
                    return false;
                }

                tipoEvento = tipoEvento.Trim().ToUpperInvariant();

                if (tipoEvento != "SUBIDA" &&
                    tipoEvento != "BAJADA" &&
                    tipoEvento != "NO_ABORDO" &&
                    tipoEvento != "AUSENTE")
                {
                    TempData["error"] = "Tipo de evento no válido.";
                    return false;
                }

                if (tipoParaderoNormalizado == "SUBIDA" && tipoEvento == "BAJADA")
                {
                    TempData["error"] = "En recojo no se puede registrar BAJADA.";
                    return false;
                }

                if (tipoParaderoNormalizado == "BAJADA" && tipoEvento == "SUBIDA")
                {
                    TempData["error"] = "En entrega no se puede registrar SUBIDA.";
                    return false;
                }
            }

            return true;
        }

        private static RecorridoResumenConductorViewModel CrearResumenRecorrido(RecorridoBE recorrido)
        {
            string turno = "Recorrido";

            if (recorrido.HoraInicioProgramada.HasValue)
            {
                if (recorrido.HoraInicioProgramada.Value.Hours < 12)
                    turno = "Recojo - Mañana";
                else
                    turno = "Entrega - Tarde";
            }

            return new RecorridoResumenConductorViewModel
            {
                IdRecorrido = recorrido.IdRecorrido,
                CodigoRecorrido = recorrido.CodigoRecorrido,
                NombreRuta = recorrido.Ruta?.Nombre ?? "Sin ruta",
                CodigoBus = recorrido.Bus?.CodigoBus ?? "Sin bus",
                HoraInicioProgramada = recorrido.HoraInicioProgramada,
                HoraFinProgramada = recorrido.HoraFinProgramada,
                EstadoRecorrido = recorrido.EstadoRecorrido,
                TurnoTexto = turno
            };
        }

        private static ParaderoResumenConductorViewModel CrearResumenParadero(ParaderoConductorBE paradero)
        {
            return new ParaderoResumenConductorViewModel
            {
                IdParadero = paradero.IdParadero,
                Nombre = paradero.Nombre,
                Direccion = paradero.Direccion,
                OrdenParada = paradero.OrdenParada,
                HoraEstimada = paradero.HoraEstimada,
                TotalAlumnos = paradero.TotalAlumnos,
                TotalRegistrados = paradero.TotalRegistrados,
                Completado = paradero.Completado
            };
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

                    var resumen = _eventoAbordajeBC.ObtenerResumenPorEstudianteRecorrido(
                        recorridoActivo.IdRecorrido,
                        idEstudiante
                    );

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

        private static ConductorAbordajeAlumnoEstadoViewModel MapearEstadoAbordaje(
            SelectListItem estudiante,
            EventoAbordajeResumenBE resumen)
        {
            bool tieneSubida = resumen.TotalSubidas > 0;
            bool tieneBajada = resumen.TotalBajadas > 0;
            bool ausente = resumen.TotalAusentes > 0;
            bool noAbordo = resumen.TotalNoAbordo > 0;

            string estadoActual = "SIN_EVENTOS";

            if (ausente)
                estadoActual = "AUSENTE";
            else if (noAbordo)
                estadoActual = "NO_ABORDO";
            else if (tieneSubida && tieneBajada)
                estadoActual = "COMPLETADO";
            else if (tieneSubida)
                estadoActual = "EN_BUS";
            else
                estadoActual = "PENDIENTE";

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

        private RecorridoBE? ObtenerRecorridoOperacion(Guid idConductor)
        {
            RecorridoBE? recorridoActivo = _recorridoBC.ObtenerActivoPorConductor(idConductor);

            if (recorridoActivo != null)
                return recorridoActivo;

            return _recorridoBC.ListarHoyPorConductor(idConductor, DateTime.Today)
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

            return Guid.TryParse(usuarioId, out Guid idUsuario)
                ? idUsuario
                : null;
        }

        private string? ObtenerIpCliente()
        {
            return HttpContext.Connection.RemoteIpAddress?.ToString();
        }

        private string? ObtenerUserAgent()
        {
            return Request.Headers.UserAgent.ToString();
        }

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

            return recorrido == null
                ? null
                : _conductorBC.ListarPorId(recorrido.IdConductor);
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