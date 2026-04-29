using CapiMovil.BL.BC;
using CapiMovil.BL.BE;
using CapiMovil.PL.Gui.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CapiMovil.PL.Gui.Controllers
{
    public class ConductorController : Controller
    {
        private readonly ConductorBC _conductorBC;
        private readonly UsuarioBC _usuarioBC;
        private readonly RecorridoBC _recorridoBC;
        private readonly IncidenciaBC _incidenciaBC;

        public ConductorController(
        ConductorBC conductorBC,
        UsuarioBC usuarioBC,
        RecorridoBC recorridoBC,
        IncidenciaBC incidenciaBC)
        {
            _conductorBC = conductorBC;
            _usuarioBC = usuarioBC;
            _recorridoBC = recorridoBC;
            _incidenciaBC = incidenciaBC;
        }

        [HttpGet]
        public IActionResult Index(Guid? idRecorridoOperacion, Guid? idParaderoActual, string? tipoParadero)
        {
            string? usuarioId = HttpContext.Session.GetString("UsuarioId");
            string? rol = HttpContext.Session.GetString("RolNombre");

            if (string.IsNullOrEmpty(usuarioId))
                return RedirectToAction("Login", "Auth");

            if ((rol ?? "").Trim().ToUpperInvariant() != "CONDUCTOR")
                return RedirectToAction("AccesoDenegado", "Auth");

            ConductorDashboardViewModel vm = new ConductorDashboardViewModel
            {
                NombreConductor = HttpContext.Session.GetString("Username") ?? "Conductor"
            };

            try
            {
                Guid idUsuario = Guid.Parse(usuarioId);

                ConductorBE? conductor = _conductorBC.ObtenerPorIdUsuario(idUsuario);

                if (conductor == null)
                    return View(vm);

                vm.NombreConductor = conductor.NombreCompleto;

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
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult IniciarRecorrido(Guid idRecorrido)
        {
            try
            {
                bool puedeIniciar = _recorridoBC.PuedeIniciarSegunOrden(idRecorrido);

                if (!puedeIniciar)
                {
                    TempData["error"] = "No puedes iniciar este recorrido todavía. Primero debes finalizar el recorrido anterior.";
                    return RedirectToAction(nameof(Index));
                }

                bool ok = _recorridoBC.Iniciar(idRecorrido);

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
            try
            {
                bool ok = _recorridoBC.Finalizar(idRecorrido);

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
            try
            {
                string? usuarioId = HttpContext.Session.GetString("UsuarioId");

                if (string.IsNullOrEmpty(usuarioId))
                    return RedirectToAction("Login", "Auth");

                Guid registradoPor = Guid.Parse(usuarioId);

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
                        registradoPor,
                        tipoEvento,
                        null
                    );
                }

                if (tipoParaderoNormalizado == "SUBIDA")
                {
                    TempData["ok"] = "Paradero de recojo registrado correctamente. Selecciona manualmente el siguiente paradero.";
                    return RedirectToAction(nameof(Index));
                }

                if (tipoParaderoNormalizado == "BAJADA")
                {
                    TempData["ok"] = "Paradero de entrega registrado correctamente. Selecciona manualmente el siguiente paradero.";
                    return RedirectToAction(nameof(Index));
                }

                TempData["ok"] = "Eventos registrados correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
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

        // ====================== MÉTODOS ORIGINALES DE ADMINISTRACIÓN ======================

        [HttpGet]
        public IActionResult Listar()
        {
            var lista = _conductorBC.Listar();
            return View(lista);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            ConductorFormViewModel vm = new ConductorFormViewModel
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
            if (!ModelState.IsValid)
            {
                vm.Usuarios = ObtenerUsuarios();
                return View(vm);
            }

            if (vm.IdUsuario == Guid.Empty)
                ModelState.AddModelError(nameof(vm.IdUsuario), "Debe seleccionar un usuario.");

            try
            {
                ConductorBE entidad = new ConductorBE
                {
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

                bool ok = _conductorBC.Registrar(entidad);

                TempData[ok ? "ok" : "error"] = ok
                    ? "Conductor registrado correctamente."
                    : "No se pudo registrar el conductor.";

                if (ok)
                    return RedirectToAction(nameof(Listar));
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }

            vm.Usuarios = ObtenerUsuarios();
            return View(vm);
        }

        [HttpGet]
        public IActionResult Editar(Guid id)
        {
            var entidad = _conductorBC.ListarPorId(id);

            if (entidad == null)
            {
                TempData["error"] = "Conductor no encontrado.";
                return RedirectToAction(nameof(Listar));
            }

            ConductorFormViewModel vm = new ConductorFormViewModel
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
                Usuarios = ObtenerUsuarios()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(ConductorFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Usuarios = ObtenerUsuarios();
                return View(vm);
            }

            try
            {
                ConductorBE entidad = new ConductorBE
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
                TempData["error"] = ex.Message;
            }

            vm.Usuarios = ObtenerUsuarios();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(Guid id)
        {
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

        private List<SelectListItem> ObtenerUsuarios()
        {
            var usuarios = _usuarioBC.Listar();

            return usuarios
                .Select(u => new SelectListItem
                {
                    Value = u.IdUsuario.ToString(),
                    Text = $"{u.Username} - {u.Correo}"
                })
                .ToList();
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RegistrarIncidenciaConductor(
    Guid idRecorrido,
    string tipoIncidencia,
    string prioridad,
    string descripcion)
        {
            try
            {
                string? usuarioId = HttpContext.Session.GetString("UsuarioId");

                if (string.IsNullOrEmpty(usuarioId))
                    return RedirectToAction("Login", "Auth");

                Guid idUsuario = Guid.Parse(usuarioId);

                ConductorBE? conductor = _conductorBC.ObtenerPorIdUsuario(idUsuario);

                if (conductor == null)
                {
                    TempData["error"] = "No se encontró el conductor asociado al usuario.";
                    return RedirectToAction(nameof(Index));
                }

                if (idRecorrido == Guid.Empty)
                {
                    TempData["error"] = "No hay recorrido seleccionado para registrar la incidencia.";
                    return RedirectToAction(nameof(Index));
                }

                string tipo = (tipoIncidencia ?? "").Trim().ToUpperInvariant();
                string nivelPrioridad = (prioridad ?? "").Trim().ToUpperInvariant();
                string detalle = (descripcion ?? "").Trim();

                if (string.IsNullOrWhiteSpace(tipo))
                {
                    TempData["error"] = "Debe seleccionar el tipo de incidencia.";
                    return RedirectToAction(nameof(Index));
                }

                if (string.IsNullOrWhiteSpace(detalle))
                {
                    TempData["error"] = "Debe ingresar una descripción.";
                    return RedirectToAction(nameof(Index));
                }

                if (string.IsNullOrWhiteSpace(nivelPrioridad))
                    nivelPrioridad = "MEDIA";

                IncidenciaBE incidencia = new IncidenciaBE
                {
                    IdRecorrido = idRecorrido,
                    IdConductor = conductor.IdConductor,
                    ReportadoPor = idUsuario,
                    TipoIncidencia = tipo,
                    Descripcion = detalle,
                    FechaHora = DateTime.Now,
                    EstadoIncidencia = "PENDIENTE",
                    Prioridad = nivelPrioridad,
                    Solucion = null
                };

                bool ok = _incidenciaBC.Registrar(incidencia);

                TempData[ok ? "ok" : "error"] = ok
                    ? "Incidencia registrada correctamente. Se notificará a los padres del recorrido."
                    : "No se pudo registrar la incidencia.";
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public IActionResult MiBus()
        {
            IActionResult? acceso = ValidarAccesoConductor();

            if (acceso != null)
                return acceso;

            ConductorDashboardViewModel vm = ConstruirDashboardConductor();
            return View(vm);
        }

        [HttpGet]
        public IActionResult MiRuta()
        {
            IActionResult? acceso = ValidarAccesoConductor();

            if (acceso != null)
                return acceso;

            ConductorDashboardViewModel vm = ConstruirDashboardConductor();
            return View(vm);
        }

        [HttpGet]
        public IActionResult Estudiantes()
        {
            IActionResult? acceso = ValidarAccesoConductor();

            if (acceso != null)
                return acceso;

            ConductorDashboardViewModel vm = ConstruirDashboardConductor();
            return View(vm);
        }

        [HttpGet]
        public IActionResult Recorrido()
        {
            IActionResult? acceso = ValidarAccesoConductor();

            if (acceso != null)
                return acceso;

            ConductorDashboardViewModel vm = ConstruirDashboardConductor();
            return View(vm);
        }

        [HttpGet]
        public IActionResult Incidencias()
        {
            IActionResult? acceso = ValidarAccesoConductor();

            if (acceso != null)
                return acceso;

            ConductorDashboardViewModel vm = ConstruirDashboardConductor();

            if (vm.RecorridoActual != null)
            {
                vm.IncidenciasPendientes = _incidenciaBC.Listar()
                    .Where(i => i.IdRecorrido == vm.RecorridoActual.IdRecorrido)
                    .OrderByDescending(i => i.FechaHora)
                    .Select(i => new IncidenciaResumenConductorViewModel
                    {
                        TipoIncidencia = i.TipoIncidencia,
                        Descripcion = i.Descripcion,
                        Prioridad = i.Prioridad,
                        FechaHora = i.FechaHora
                    })
                    .ToList();
            }

            return View(vm);
        }
        private IActionResult? ValidarAccesoConductor()
        {
            string? usuarioId = HttpContext.Session.GetString("UsuarioId");
            string? rol = HttpContext.Session.GetString("RolNombre");

            if (string.IsNullOrEmpty(usuarioId))
                return RedirectToAction("Login", "Auth");

            if ((rol ?? "").Trim().ToUpperInvariant() != "CONDUCTOR")
                return RedirectToAction("AccesoDenegado", "Auth");

            return null;
        }

        private ConductorDashboardViewModel ConstruirDashboardConductor()
        {
            ConductorDashboardViewModel vm = new ConductorDashboardViewModel
            {
                NombreConductor = HttpContext.Session.GetString("Username") ?? "Conductor"
            };

            string? usuarioId = HttpContext.Session.GetString("UsuarioId");

            if (string.IsNullOrEmpty(usuarioId))
                return vm;

            Guid idUsuario = Guid.Parse(usuarioId);

            ConductorBE? conductor = _conductorBC.ObtenerPorIdUsuario(idUsuario);

            if (conductor == null)
                return vm;

            vm.NombreConductor = conductor.NombreCompleto;

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
    }
}