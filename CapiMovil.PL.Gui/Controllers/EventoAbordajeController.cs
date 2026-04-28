using CapiMovil.BL.BC;
using CapiMovil.BL.BE;
using CapiMovil.DL.DALC;
using CapiMovil.PL.Gui.Infrastructure;
using CapiMovil.PL.Gui.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CapiMovil.PL.Gui.Controllers
{
    public class EventoAbordajeController : Controller
    {
        private readonly EventoAbordajeBC _eventoAbordajeBC;
        private readonly RecorridoDALC _recorridoDALC;
        private readonly EstudianteDALC _estudianteDALC;
        private readonly ParaderoDALC _paraderoDALC;
        private readonly RutaDALC _rutaDALC;
        private readonly ConductorBC _conductorBC;
        private readonly RutaEstudianteBC _rutaEstudianteBC;
        private readonly UbicacionBusBC _ubicacionBusBC;
        private readonly IncidenciaBC _incidenciaBC;

        public EventoAbordajeController(
            EventoAbordajeBC eventoAbordajeBC,
            RecorridoDALC recorridoDALC,
            EstudianteDALC estudianteDALC,
            ParaderoDALC paraderoDALC,
            RutaDALC rutaDALC,
            ConductorBC conductorBC,
            RutaEstudianteBC rutaEstudianteBC,
            UbicacionBusBC ubicacionBusBC,
            IncidenciaBC incidenciaBC)
        {
            _eventoAbordajeBC = eventoAbordajeBC;
            _recorridoDALC = recorridoDALC;
            _estudianteDALC = estudianteDALC;
            _paraderoDALC = paraderoDALC;
            _rutaDALC = rutaDALC;
            _conductorBC = conductorBC;
            _rutaEstudianteBC = rutaEstudianteBC;
            _ubicacionBusBC = ubicacionBusBC;
            _incidenciaBC = incidenciaBC;
        }

        public IActionResult Listar(DateTime? fecha, Guid? idRuta, Guid? idRecorrido, Guid? idConductor, string? estadoRecorrido, string? estadoAbordaje)
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Administracion);
            if (acceso != null) return acceso;

            DateTime fechaFiltro = (fecha ?? DateTime.Today).Date;
            string? estadoRecorridoFiltro = string.IsNullOrWhiteSpace(estadoRecorrido) ? null : estadoRecorrido.Trim().ToUpperInvariant();
            string? estadoAbordajeFiltro = string.IsNullOrWhiteSpace(estadoAbordaje) ? null : estadoAbordaje.Trim().ToUpperInvariant();

            List<RecorridoBE> recorridos = _recorridoDALC.Listar();
            List<EventoAbordajeBE> eventos = _eventoAbordajeBC.Listar();

            List<RecorridoBE> recorridosDia = recorridos
                .Where(r => r.Estado && r.Fecha.Date == fechaFiltro)
                .ToList();

            if (idRuta.HasValue && idRuta.Value != Guid.Empty)
                recorridosDia = recorridosDia.Where(r => r.IdRuta == idRuta.Value).ToList();

            if (idConductor.HasValue && idConductor.Value != Guid.Empty)
                recorridosDia = recorridosDia.Where(r => r.IdConductor == idConductor.Value).ToList();

            if (!string.IsNullOrWhiteSpace(estadoRecorridoFiltro))
                recorridosDia = recorridosDia.Where(r => string.Equals(r.EstadoRecorrido, estadoRecorridoFiltro, StringComparison.OrdinalIgnoreCase)).ToList();

            if (idRecorrido.HasValue && idRecorrido.Value != Guid.Empty)
                recorridosDia = recorridosDia.Where(r => r.IdRecorrido == idRecorrido.Value).ToList();

            HashSet<Guid> idsRecorridosDia = recorridosDia.Select(r => r.IdRecorrido).ToHashSet();
            List<EventoAbordajeBE> eventosDia = eventos
                .Where(e => idsRecorridosDia.Contains(e.IdRecorrido))
                .ToList();

            if (!string.IsNullOrWhiteSpace(estadoAbordajeFiltro))
                eventosDia = eventosDia.Where(e => string.Equals(e.TipoEvento, estadoAbordajeFiltro, StringComparison.OrdinalIgnoreCase)).ToList();

            RecorridoBE? recorridoSeleccionado = recorridosDia
                .OrderByDescending(r => r.Fecha)
                .ThenByDescending(r => r.FechaCreacion)
                .FirstOrDefault();

            if (idRecorrido.HasValue && idRecorrido.Value != Guid.Empty)
                recorridoSeleccionado = recorridosDia.FirstOrDefault(r => r.IdRecorrido == idRecorrido.Value);

            List<AdminAbordajeParaderoViewModel> paraderosVm = new();
            List<AdminAbordajeBusMapaItemViewModel> busesMapa = new();
            AdminAbordajeRecorridoDetalleViewModel? detalle = null;

            if (recorridoSeleccionado != null)
            {
                List<ParaderoBE> paraderos = _paraderoDALC.ListarPorRuta(recorridoSeleccionado.IdRuta)
                    .OrderBy(x => x.OrdenParada)
                    .ToList();

                List<RutaEstudianteBE> asignacionesRuta = _rutaEstudianteBC.Listar()
                    .Where(x => x.Estado && x.IdRuta == recorridoSeleccionado.IdRuta)
                    .ToList();

                foreach (ParaderoBE p in paraderos)
                {
                    int totalParadero = asignacionesRuta.Count(a => a.IdParaderoSubida == p.IdParadero);
                    int subidas = eventosDia.Count(e => e.IdRecorrido == recorridoSeleccionado.IdRecorrido && e.IdParadero == p.IdParadero && e.TipoEvento == "SUBIDA");
                    int bajadas = eventosDia.Count(e => e.IdRecorrido == recorridoSeleccionado.IdRecorrido && e.IdParadero == p.IdParadero && e.TipoEvento == "BAJADA");
                    int ausentes = eventosDia.Count(e => e.IdRecorrido == recorridoSeleccionado.IdRecorrido && e.IdParadero == p.IdParadero && e.TipoEvento == "AUSENTE");
                    int noAbordo = eventosDia.Count(e => e.IdRecorrido == recorridoSeleccionado.IdRecorrido && e.IdParadero == p.IdParadero && e.TipoEvento == "NO_ABORDO");
                    int pendientes = Math.Max(totalParadero - (subidas + ausentes + noAbordo), 0);

                    paraderosVm.Add(new AdminAbordajeParaderoViewModel
                    {
                        IdParadero = p.IdParadero,
                        Nombre = p.Nombre,
                        Direccion = p.Direccion ?? "Sin dirección",
                        OrdenParada = p.OrdenParada,
                        HoraEstimada = p.HoraEstimada,
                        Latitud = p.Latitud,
                        Longitud = p.Longitud,
                        TotalEstudiantes = totalParadero,
                        Subidas = subidas,
                        Bajadas = bajadas,
                        Ausentes = ausentes,
                        Pendientes = pendientes,
                        NoAbordo = noAbordo
                    });
                }

                UbicacionBusBE? ubicacion = _ubicacionBusBC.Listar()
                    .Where(x => x.Estado && x.IdRecorrido == recorridoSeleccionado.IdRecorrido)
                    .OrderByDescending(x => x.FechaHora)
                    .FirstOrDefault();

                if (ubicacion != null)
                {
                    busesMapa.Add(new AdminAbordajeBusMapaItemViewModel
                    {
                        Latitud = ubicacion.Latitud,
                        Longitud = ubicacion.Longitud,
                        Bus = recorridoSeleccionado.Bus?.Placa ?? "Bus",
                        Conductor = recorridoSeleccionado.Conductor?.NombreCompleto ?? "Conductor",
                        Recorrido = recorridoSeleccionado.CodigoRecorrido,
                        FechaHora = ubicacion.FechaHora
                    });
                }

                detalle = new AdminAbordajeRecorridoDetalleViewModel
                {
                    IdRecorrido = recorridoSeleccionado.IdRecorrido,
                    CodigoRecorrido = recorridoSeleccionado.CodigoRecorrido,
                    Ruta = $"{recorridoSeleccionado.Ruta?.CodigoRuta} - {recorridoSeleccionado.Ruta?.Nombre}",
                    Bus = recorridoSeleccionado.Bus?.Placa ?? "No asignado",
                    Conductor = recorridoSeleccionado.Conductor?.NombreCompleto ?? "No asignado",
                    Fecha = recorridoSeleccionado.Fecha,
                    HoraInicioProgramada = recorridoSeleccionado.HoraInicioProgramada,
                    HoraInicioReal = recorridoSeleccionado.HoraInicioReal,
                    HoraFinReal = recorridoSeleccionado.HoraFinReal,
                    EstadoRecorrido = recorridoSeleccionado.EstadoRecorrido ?? "SIN_ESTADO",
                    LatitudInicioRuta = _rutaDALC.ListarPorId(recorridoSeleccionado.IdRuta)?.LatitudInicio,
                    LongitudInicioRuta = _rutaDALC.ListarPorId(recorridoSeleccionado.IdRuta)?.LongitudInicio,
                    LatitudFinRuta = _rutaDALC.ListarPorId(recorridoSeleccionado.IdRuta)?.LatitudFin,
                    LongitudFinRuta = _rutaDALC.ListarPorId(recorridoSeleccionado.IdRuta)?.LongitudFin
                };
            }

            List<AdminAbordajeTimelineItemViewModel> timeline = eventosDia
                .OrderByDescending(e => e.FechaHora)
                .Take(120)
                .Select(e => new AdminAbordajeTimelineItemViewModel
                {
                    FechaHora = e.FechaHora,
                    Alumno = $"{e.Estudiante?.CodigoEstudiante} - {e.Estudiante?.Nombres} {e.Estudiante?.ApellidoPaterno}".Trim(),
                    TipoEvento = e.TipoEvento,
                    Paradero = e.Paradero?.Nombre ?? "Sin paradero",
                    Recorrido = e.Recorrido?.CodigoRecorrido ?? "Sin recorrido"
                }).ToList();

            HashSet<Guid> idsRuta = recorridosDia.Select(r => r.IdRuta).ToHashSet();
            int totalEstudiantesAsignados = _rutaEstudianteBC.Listar()
                .Count(x => x.Estado && idsRuta.Contains(x.IdRuta));

            int totalIncidencias = _incidenciaBC.Listar()
                .Count(i => idsRecorridosDia.Contains(i.IdRecorrido));

            AdminAbordajeDashboardViewModel vm = new()
            {
                Fecha = fechaFiltro,
                IdRuta = idRuta,
                IdRecorrido = recorridoSeleccionado?.IdRecorrido ?? idRecorrido,
                IdConductor = idConductor,
                EstadoRecorrido = estadoRecorridoFiltro,
                EstadoAbordaje = estadoAbordajeFiltro,

                TotalRecorridosDia = recorridosDia.Count,
                TotalEstudiantesAsignados = totalEstudiantesAsignados,
                TotalAbordajesRegistrados = eventosDia.Count,
                TotalSubidas = eventosDia.Count(e => e.TipoEvento == "SUBIDA"),
                TotalBajadas = eventosDia.Count(e => e.TipoEvento == "BAJADA"),
                TotalAusentes = eventosDia.Count(e => e.TipoEvento == "AUSENTE"),
                TotalNoAbordo = eventosDia.Count(e => e.TipoEvento == "NO_ABORDO"),
                TotalIncidenciasRelacionadas = totalIncidencias,

                RecorridoSeleccionado = detalle,
                Paraderos = paraderosVm,
                Timeline = timeline,
                BusesMapa = busesMapa,

                Rutas = _rutaDALC.ListarActivas().Select(x => new SelectListItem(x.CodigoRuta + " - " + x.Nombre, x.IdRuta.ToString(), x.IdRuta == idRuta)).ToList(),
                Recorridos = recorridosDia.Select(x => new SelectListItem(x.CodigoRecorrido, x.IdRecorrido.ToString(), recorridoSeleccionado != null && x.IdRecorrido == recorridoSeleccionado.IdRecorrido)).ToList(),
                Conductores = recorridos.Select(x => x.Conductor).Where(c => c != null).GroupBy(c => c!.IdConductor).Select(g => g.First()!).Select(c => new SelectListItem($"{c.CodigoConductor} - {c.NombreCompleto}", c.IdConductor.ToString(), c.IdConductor == idConductor)).ToList(),
                EstadosRecorrido = new List<SelectListItem>
                {
                    new("PROGRAMADO","PROGRAMADO", estadoRecorridoFiltro == "PROGRAMADO"),
                    new("EN_CURSO","EN_CURSO", estadoRecorridoFiltro == "EN_CURSO"),
                    new("FINALIZADO","FINALIZADO", estadoRecorridoFiltro == "FINALIZADO"),
                    new("CANCELADO","CANCELADO", estadoRecorridoFiltro == "CANCELADO")
                },
                EstadosAbordaje = new List<SelectListItem>
                {
                    new("SUBIDA","SUBIDA", estadoAbordajeFiltro == "SUBIDA"),
                    new("BAJADA","BAJADA", estadoAbordajeFiltro == "BAJADA"),
                    new("AUSENTE","AUSENTE", estadoAbordajeFiltro == "AUSENTE"),
                    new("NO_ABORDO","NO_ABORDO", estadoAbordajeFiltro == "NO_ABORDO")
                }
            };

            return View(vm);
        }

        [HttpGet]
        public IActionResult Crear(Guid? idRecorrido, Guid? idParadero, string? estado)
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Conductor);
            if (acceso != null) return acceso;

            ConductorBE? conductorSesion = ObtenerConductorSesion();
            if (conductorSesion == null)
            {
                TempData["error"] = "No se encontró el perfil de conductor asociado a su sesión.";
                return RedirectToAction("AccesoDenegado", "Auth");
            }

            ConductorAbordajeOperacionViewModel vm = ConstruirVistaOperacionConductor(
                conductorSesion.IdConductor,
                conductorSesion.NombreCompleto,
                idRecorrido,
                idParadero,
                estado);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(EventoAbordajeFormViewModel vm)
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Conductor);
            if (acceso != null) return acceso;

            ConductorBE? conductorSesion = ObtenerConductorSesion();
            if (conductorSesion == null)
            {
                TempData["error"] = "No se encontró el perfil de conductor asociado a su sesión.";
                return RedirectToAction("AccesoDenegado", "Auth");
            }

            if (vm.IdRecorrido == Guid.Empty)
                ModelState.AddModelError(nameof(vm.IdRecorrido), "Debe seleccionar un recorrido.");

            if (vm.IdEstudiante == Guid.Empty)
                ModelState.AddModelError(nameof(vm.IdEstudiante), "Debe seleccionar un estudiante.");

            bool recorridoValido = _recorridoDALC.ListarPorConductor(conductorSesion.IdConductor)
                .Any(x => x.IdRecorrido == vm.IdRecorrido);

            if (!recorridoValido)
                ModelState.AddModelError(nameof(vm.IdRecorrido), "Solo puede registrar eventos en recorridos asignados a su usuario.");

            if (!ModelState.IsValid)
            {
                TempData["error"] = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(x => x.ErrorMessage));
                return RedirectToAction(nameof(Crear), new { idRecorrido = vm.IdRecorrido });
            }

            try
            {
                string? usuarioIdSession = HttpContext.Session.GetString("UsuarioId");
                Guid? usuarioId = string.IsNullOrWhiteSpace(usuarioIdSession) ? null : Guid.Parse(usuarioIdSession);

                EventoAbordajeBE entidad = new()
                {
                    IdRecorrido = vm.IdRecorrido,
                    IdEstudiante = vm.IdEstudiante,
                    IdParadero = vm.IdParadero,
                    RegistradoPor = usuarioId,
                    TipoEvento = vm.TipoEvento,
                    FechaHora = DateTime.Now,
                    Observacion = vm.Observacion,
                    Estado = vm.Estado
                };

                bool ok = _eventoAbordajeBC.Registrar(entidad);

                TempData[ok ? "ok" : "error"] = ok
                    ? $"Evento registrado correctamente. Código generado: {entidad.CodigoEvento}"
                    : "No se pudo registrar el evento.";

                if (ok)
                    return RedirectToAction(nameof(Crear), new { idRecorrido = vm.IdRecorrido });
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }

            return RedirectToAction(nameof(Crear), new { idRecorrido = vm.IdRecorrido });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RegistrarRapido(Guid idRecorrido, Guid idEstudiante, Guid? idParadero, string tipoEvento, Guid? idParaderoFiltro, string? estadoFiltro)
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Conductor);
            if (acceso != null) return acceso;

            ConductorBE? conductorSesion = ObtenerConductorSesion();
            if (conductorSesion == null)
            {
                TempData["error"] = "No se encontró el perfil de conductor asociado a su sesión.";
                return RedirectToAction("AccesoDenegado", "Auth");
            }

            bool recorridoValido = _recorridoDALC.ListarPorConductor(conductorSesion.IdConductor)
                .Any(x => x.IdRecorrido == idRecorrido);

            if (!recorridoValido)
            {
                TempData["error"] = "No tiene permiso para registrar eventos en ese recorrido.";
                return RedirectToAction(nameof(Crear));
            }

            try
            {
                Guid? usuarioId = ObtenerUsuarioIdSesion();

                EventoAbordajeBE entidad = new()
                {
                    IdRecorrido = idRecorrido,
                    IdEstudiante = idEstudiante,
                    IdParadero = idParadero,
                    RegistradoPor = usuarioId,
                    TipoEvento = (tipoEvento ?? string.Empty).Trim().ToUpperInvariant(),
                    FechaHora = DateTime.Now,
                    Estado = true
                };

                bool ok = _eventoAbordajeBC.Registrar(entidad);
                TempData[ok ? "ok" : "error"] = ok
                    ? $"Evento {entidad.TipoEvento} registrado: {entidad.CodigoEvento} ({entidad.FechaHora:dd/MM/yyyy HH:mm:ss})"
                    : "No se pudo registrar el evento.";
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }

            return RedirectToAction(nameof(Crear), new
            {
                idRecorrido,
                idParadero = idParaderoFiltro,
                estado = estadoFiltro
            });
        }

        [HttpGet]
        public IActionResult Editar(Guid id)
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Conductor);
            if (acceso != null) return acceso;

            var entidad = _eventoAbordajeBC.ListarPorId(id);

            if (entidad == null)
            {
                TempData["error"] = "Evento no encontrado.";
                return RedirectToAction(nameof(Listar));
            }

            EventoAbordajeFormViewModel vm = new()
            {
                IdEvento = entidad.IdEvento,
                IdRecorrido = entidad.IdRecorrido,
                IdEstudiante = entidad.IdEstudiante,
                IdParadero = entidad.IdParadero,
                RegistradoPor = entidad.RegistradoPor,
                CodigoEvento = entidad.CodigoEvento,
                TipoEvento = entidad.TipoEvento,
                FechaHora = entidad.FechaHora,
                Observacion = entidad.Observacion,
                Estado = entidad.Estado,
                Recorridos = ObtenerRecorridosParaEdicion(entidad.IdRecorrido),
                Estudiantes = ObtenerEstudiantes(),
                Paraderos = ObtenerParaderos(),
                TiposEvento = ObtenerTiposEvento()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(EventoAbordajeFormViewModel vm)
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Conductor);
            if (acceso != null) return acceso;

            if (vm.IdRecorrido == Guid.Empty)
                ModelState.AddModelError(nameof(vm.IdRecorrido), "Debe seleccionar un recorrido.");

            if (vm.IdEstudiante == Guid.Empty)
                ModelState.AddModelError(nameof(vm.IdEstudiante), "Debe seleccionar un estudiante.");

            if (!ModelState.IsValid)
            {
                vm.Recorridos = ObtenerRecorridosParaEdicion(vm.IdRecorrido);
                vm.Estudiantes = ObtenerEstudiantes();
                vm.Paraderos = ObtenerParaderos();
                vm.TiposEvento = ObtenerTiposEvento();
                return View(vm);
            }

            try
            {
                EventoAbordajeBE entidad = new()
                {
                    IdEvento = vm.IdEvento,
                    IdRecorrido = vm.IdRecorrido,
                    IdEstudiante = vm.IdEstudiante,
                    IdParadero = vm.IdParadero,
                    RegistradoPor = vm.RegistradoPor,
                    TipoEvento = vm.TipoEvento,
                    FechaHora = vm.FechaHora,
                    Observacion = vm.Observacion,
                    Estado = vm.Estado
                };

                bool ok = _eventoAbordajeBC.Actualizar(entidad);

                TempData[ok ? "ok" : "error"] = ok
                    ? "Evento actualizado correctamente."
                    : "No se pudo actualizar el evento.";

                if (ok)
                    return RedirectToAction(nameof(Listar));
            }
            catch (Exception ex)
            {
                ViewBag.SwalError = ex.Message;
            }

            vm.Recorridos = ObtenerRecorridosParaEdicion(vm.IdRecorrido);
            vm.Estudiantes = ObtenerEstudiantes();
            vm.Paraderos = ObtenerParaderos();
            vm.TiposEvento = ObtenerTiposEvento();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(Guid id)
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Conductor);
            if (acceso != null) return acceso;

            try
            {
                bool ok = _eventoAbordajeBC.Eliminar(id);

                TempData[ok ? "ok" : "error"] = ok
                    ? "Evento eliminado correctamente."
                    : "No se pudo eliminar el evento.";
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }

            return RedirectToAction(nameof(Listar));
        }

        private List<SelectListItem> ObtenerRecorridos()
        {
            return _recorridoDALC.ListarActivosParaOperacion()
                .Select(x => new SelectListItem
                {
                    Value = x.IdRecorrido.ToString(),
                    Text = $"{x.CodigoRecorrido} - {x.Fecha:dd/MM/yyyy}"
                })
                .ToList();
        }

        private List<SelectListItem> ObtenerRecorridosParaEdicion(Guid? idSeleccionado = null)
        {
            var lista = ObtenerRecorridos();

            if (idSeleccionado.HasValue && idSeleccionado.Value != Guid.Empty)
            {
                bool existe = lista.Any(x => x.Value == idSeleccionado.Value.ToString());

                if (!existe)
                {
                    var recorridoActual = _recorridoDALC.ListarPorId(idSeleccionado.Value);

                    if (recorridoActual != null)
                    {
                        lista.Insert(0, new SelectListItem
                        {
                            Value = recorridoActual.IdRecorrido.ToString(),
                            Text = $"{recorridoActual.CodigoRecorrido} - {recorridoActual.Fecha:dd/MM/yyyy}"
                        });
                    }
                }
            }

            return lista;
        }
        private List<SelectListItem> ObtenerEstudiantes()
        {
            return _estudianteDALC.ListarActivos()
                .Select(x => new SelectListItem
                {
                    Value = x.IdEstudiante.ToString(),
                    Text = $"{x.CodigoEstudiante} - {x.Nombres} {x.ApellidoPaterno}"
                }).ToList();
        }

        private List<SelectListItem> ObtenerParaderos()
        {
            return _paraderoDALC.ListarActivos()
                .Select(x => new SelectListItem
                {
                    Value = x.IdParadero.ToString(),
                    Text = $"{x.CodigoParadero} - {x.Nombre}"
                }).ToList();
        }

        private List<SelectListItem> ObtenerTiposEvento()
        {
            return new List<SelectListItem>
            {
                new("SUBIDA", "SUBIDA"),
                new("BAJADA", "BAJADA"),
                new("AUSENTE", "AUSENTE"),
                new("NO_ABORDO", "NO_ABORDO")
            };
        }

        private Guid? ObtenerUsuarioIdSesion()
        {
            string? usuarioIdSession = HttpContext.Session.GetString("UsuarioId");
            return Guid.TryParse(usuarioIdSession, out Guid usuarioId) ? usuarioId : null;
        }

        private ConductorBE? ObtenerConductorSesion()
        {
            Guid? idUsuario = ObtenerUsuarioIdSesion();
            if (!idUsuario.HasValue)
                return null;

            return _conductorBC.ObtenerPorIdUsuario(idUsuario.Value);
        }

        private ConductorAbordajeOperacionViewModel ConstruirVistaOperacionConductor(Guid idConductor, string nombreConductor, Guid? idRecorrido, Guid? idParadero, string? estado)
        {
            string estadoFiltro = string.IsNullOrWhiteSpace(estado) ? "PENDIENTE" : estado.Trim().ToUpperInvariant();
            HashSet<string> estadosValidos = new(StringComparer.OrdinalIgnoreCase) { "TODOS", "PENDIENTE", "SUBIDA", "BAJADA", "AUSENTE", "NO_ABORDO" };
            if (!estadosValidos.Contains(estadoFiltro))
                estadoFiltro = "PENDIENTE";

            List<RecorridoBE> recorridosConductor = _recorridoDALC.ListarPorConductor(idConductor)
                .Where(x => x.Estado)
                .OrderByDescending(x => x.Fecha)
                .ThenByDescending(x => x.FechaCreacion)
                .ToList();

            RecorridoBE? recorridoSeleccionado = null;
            if (idRecorrido.HasValue && idRecorrido.Value != Guid.Empty)
                recorridoSeleccionado = recorridosConductor.FirstOrDefault(x => x.IdRecorrido == idRecorrido.Value);

            recorridoSeleccionado ??= recorridosConductor
                .FirstOrDefault(x => string.Equals(x.EstadoRecorrido, "EN_CURSO", StringComparison.OrdinalIgnoreCase))
                ?? recorridosConductor.FirstOrDefault();

            List<SelectListItem> recorridosItems = recorridosConductor
                .Select(x => new SelectListItem
                {
                    Value = x.IdRecorrido.ToString(),
                    Text = $"{x.CodigoRecorrido} · {x.Ruta?.Nombre ?? "Sin ruta"} · {x.Fecha:dd/MM}",
                    Selected = recorridoSeleccionado != null && x.IdRecorrido == recorridoSeleccionado.IdRecorrido
                }).ToList();

            if (recorridoSeleccionado == null)
            {
                return new ConductorAbordajeOperacionViewModel
                {
                    Recorridos = recorridosItems,
                    EstadosFiltro = ConstruirFiltroEstado(estadoFiltro),
                    EstadoFiltro = estadoFiltro
                };
            }

            List<ParaderoBE> paraderosRuta = _paraderoDALC.ListarPorRuta(recorridoSeleccionado.IdRuta)
                .Where(x => x.Estado)
                .OrderBy(x => x.OrdenParada)
                .ToList();

            Dictionary<Guid, ParaderoBE> paraderoMap = paraderosRuta.ToDictionary(x => x.IdParadero, x => x);

            List<RutaEstudianteBE> asignaciones = _rutaEstudianteBC.Listar()
                .Where(x => x.Estado && x.IdRuta == recorridoSeleccionado.IdRuta)
                .ToList();

            HashSet<Guid> idsEstudiantesAsignados = asignaciones.Select(x => x.IdEstudiante).ToHashSet();
            Dictionary<Guid, EstudianteBE> estudiantesMap = _estudianteDALC.ListarActivos()
                .Where(x => idsEstudiantesAsignados.Contains(x.IdEstudiante))
                .ToDictionary(x => x.IdEstudiante, x => x);

            List<EventoAbordajeBE> eventosRecorrido = _eventoAbordajeBC.Listar()
                .Where(x => x.IdRecorrido == recorridoSeleccionado.IdRecorrido && x.Estado)
                .ToList();

            List<ConductorAbordajeAlumnoItemViewModel> alumnos = new();

            foreach (RutaEstudianteBE asignacion in asignaciones)
            {
                if (!estudiantesMap.TryGetValue(asignacion.IdEstudiante, out EstudianteBE? estudiante))
                    continue;

                EventoAbordajeResumenBE resumen = _eventoAbordajeBC.ObtenerResumenPorEstudianteRecorrido(recorridoSeleccionado.IdRecorrido, estudiante.IdEstudiante);
                string estadoActual = ResolverEstadoActual(resumen);

                string badgeClass = estadoActual switch
                {
                    "SUBIDA" => "bg-success",
                    "BAJADA" => "bg-primary",
                    "AUSENTE" => "bg-danger",
                    "NO_ABORDO" => "bg-secondary",
                    _ => "bg-warning text-dark"
                };

                bool permiteSubida = estadoActual == "PENDIENTE";
                bool permiteBajada = estadoActual == "SUBIDA";
                bool permiteAusente = estadoActual == "PENDIENTE";
                bool permiteNoAbordo = estadoActual == "PENDIENTE";

                ParaderoBE? paraderoSubida = null;
                if (asignacion.IdParaderoSubida.HasValue)
                    paraderoMap.TryGetValue(asignacion.IdParaderoSubida.Value, out paraderoSubida);

                DateTime? ultimoEvento = eventosRecorrido
                    .Where(x => x.IdEstudiante == estudiante.IdEstudiante)
                    .OrderByDescending(x => x.FechaHora)
                    .Select(x => (DateTime?)x.FechaHora)
                    .FirstOrDefault();

                alumnos.Add(new ConductorAbordajeAlumnoItemViewModel
                {
                    IdEstudiante = estudiante.IdEstudiante,
                    CodigoEstudiante = estudiante.CodigoEstudiante,
                    NombreCompleto = estudiante.NombreCompleto,
                    GradoSeccion = $"{estudiante.Grado ?? "-"} {estudiante.Seccion ?? string.Empty}".Trim(),
                    IdParaderoSubida = asignacion.IdParaderoSubida,
                    ParaderoSubida = paraderoSubida?.Nombre ?? "Sin paradero",
                    OrdenParaderoSubida = paraderoSubida?.OrdenParada ?? 9999,
                    EstadoActual = estadoActual,
                    EstadoBadgeClass = badgeClass,
                    PermiteSubida = permiteSubida,
                    PermiteBajada = permiteBajada,
                    PermiteAusente = permiteAusente,
                    PermiteNoAbordo = permiteNoAbordo,
                    UltimoEventoFechaHora = ultimoEvento
                });
            }

            List<ConductorAbordajeAlumnoItemViewModel> alumnosBase = alumnos
                .OrderBy(x => x.EstadoActual == "PENDIENTE" ? 0 : 1)
                .ThenBy(x => x.OrdenParaderoSubida)
                .ThenBy(x => x.NombreCompleto)
                .ToList();

            int totalSubieron = alumnosBase.Count(x => x.EstadoActual == "SUBIDA");
            int totalBajaron = alumnosBase.Count(x => x.EstadoActual == "BAJADA");
            int totalPendientes = alumnosBase.Count(x => x.EstadoActual == "PENDIENTE");
            int totalAusentes = alumnosBase.Count(x => x.EstadoActual == "AUSENTE");
            int totalNoAbordo = alumnosBase.Count(x => x.EstadoActual == "NO_ABORDO");

            List<SelectListItem> paraderosFiltro = new()
            {
                new SelectListItem { Value = "", Text = "Todos los paraderos", Selected = !idParadero.HasValue }
            };
            paraderosFiltro.AddRange(paraderosRuta.Select(p => new SelectListItem
            {
                Value = p.IdParadero.ToString(),
                Text = $"#{p.OrdenParada} {p.Nombre}",
                Selected = idParadero.HasValue && idParadero.Value == p.IdParadero
            }));

            string? paraderoActual = paraderosRuta
                .Select(p => new { p.Nombre, Pendientes = alumnosBase.Count(a => a.IdParaderoSubida == p.IdParadero && a.EstadoActual == "PENDIENTE") })
                .Where(x => x.Pendientes > 0)
                .OrderByDescending(x => x.Pendientes)
                .Select(x => x.Nombre)
                .FirstOrDefault();

            IEnumerable<ConductorAbordajeAlumnoItemViewModel> alumnosFiltrados = alumnosBase;
            if (idParadero.HasValue)
                alumnosFiltrados = alumnosFiltrados.Where(x => x.IdParaderoSubida == idParadero.Value);

            if (!string.Equals(estadoFiltro, "TODOS", StringComparison.OrdinalIgnoreCase))
                alumnosFiltrados = alumnosFiltrados.Where(x => string.Equals(x.EstadoActual, estadoFiltro, StringComparison.OrdinalIgnoreCase));

            List<ConductorAbordajeParaderoCardViewModel> paraderosCards = paraderosRuta
                .Select(p =>
                {
                    List<ConductorAbordajeAlumnoItemViewModel> alumnosParadero = alumnosFiltrados
                        .Where(a => a.IdParaderoSubida == p.IdParadero)
                        .ToList();

                    return new ConductorAbordajeParaderoCardViewModel
                    {
                        IdParadero = p.IdParadero,
                        OrdenParada = p.OrdenParada,
                        Nombre = p.Nombre,
                        Direccion = p.Direccion,
                        HoraEstimada = p.HoraEstimada,
                        EsParaderoActual = !string.IsNullOrWhiteSpace(paraderoActual) && string.Equals(p.Nombre, paraderoActual, StringComparison.OrdinalIgnoreCase),
                        TotalAlumnos = alumnosParadero.Count,
                        TotalSubieron = alumnosParadero.Count(x => x.EstadoActual == "SUBIDA"),
                        TotalBajaron = alumnosParadero.Count(x => x.EstadoActual == "BAJADA"),
                        TotalPendientes = alumnosParadero.Count(x => x.EstadoActual == "PENDIENTE"),
                        TotalAusentes = alumnosParadero.Count(x => x.EstadoActual == "AUSENTE"),
                        TotalNoAbordo = alumnosParadero.Count(x => x.EstadoActual == "NO_ABORDO"),
                        Alumnos = alumnosParadero
                    };
                })
                .Where(p => p.TotalAlumnos > 0 || !idParadero.HasValue)
                .OrderBy(p => p.OrdenParada)
                .ToList();

            return new ConductorAbordajeOperacionViewModel
            {
                IdRecorridoSeleccionado = recorridoSeleccionado.IdRecorrido,
                IdParaderoFiltro = idParadero,
                EstadoFiltro = estadoFiltro,
                CodigoRecorrido = recorridoSeleccionado.CodigoRecorrido,
                Ruta = $"{recorridoSeleccionado.Ruta?.CodigoRuta} - {recorridoSeleccionado.Ruta?.Nombre}",
                Bus = recorridoSeleccionado.Bus?.Placa ?? "Sin bus",
                Conductor = string.IsNullOrWhiteSpace(nombreConductor) ? "Sin conductor" : nombreConductor,
                FechaOperacion = recorridoSeleccionado.Fecha,
                EstadoRecorrido = recorridoSeleccionado.EstadoRecorrido ?? "SIN_ESTADO",
                ParaderoActual = paraderoActual,
                TotalAlumnos = asignaciones.Count,
                TotalSubieron = totalSubieron,
                TotalBajaron = totalBajaron,
                TotalPendientes = totalPendientes,
                TotalAusentes = totalAusentes,
                TotalNoAbordo = totalNoAbordo,
                Recorridos = recorridosItems,
                ParaderosFiltro = paraderosFiltro,
                EstadosFiltro = ConstruirFiltroEstado(estadoFiltro),
                Paraderos = paraderosCards
            };
        }

        private static string ResolverEstadoActual(EventoAbordajeResumenBE resumen)
        {
            if (resumen.TotalBajadas > 0) return "BAJADA";
            if (resumen.TotalSubidas > 0) return "SUBIDA";
            if (resumen.TotalAusentes > 0) return "AUSENTE";
            if (resumen.TotalNoAbordo > 0) return "NO_ABORDO";
            return "PENDIENTE";
        }

        private static List<SelectListItem> ConstruirFiltroEstado(string estadoFiltro)
        {
            return new List<SelectListItem>
            {
                new("TODOS", "Todos", string.Equals(estadoFiltro, "TODOS", StringComparison.OrdinalIgnoreCase)),
                new("PENDIENTE", "Pendientes", string.Equals(estadoFiltro, "PENDIENTE", StringComparison.OrdinalIgnoreCase)),
                new("SUBIDA", "Subieron", string.Equals(estadoFiltro, "SUBIDA", StringComparison.OrdinalIgnoreCase)),
                new("BAJADA", "Bajaron", string.Equals(estadoFiltro, "BAJADA", StringComparison.OrdinalIgnoreCase)),
                new("AUSENTE", "Ausentes", string.Equals(estadoFiltro, "AUSENTE", StringComparison.OrdinalIgnoreCase)),
                new("NO_ABORDO", "No abordó", string.Equals(estadoFiltro, "NO_ABORDO", StringComparison.OrdinalIgnoreCase))
            };
        }
    }
}
