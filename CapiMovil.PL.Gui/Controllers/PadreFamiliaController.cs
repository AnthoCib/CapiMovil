using CapiMovil.BL.BC;
using CapiMovil.BL.BE;
using CapiMovil.PL.Gui.Infrastructure;
using CapiMovil.PL.Gui.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CapiMovil.PL.Gui.Controllers
{
    public class PadreFamiliaController : Controller
    {
        private readonly PadreFamiliaBC _padreFamiliaBC;
        private readonly UsuarioBC _usuarioBC;
        private readonly EstudianteBC _estudianteBC;
        private readonly NotificacionBC _notificacionBC;
        private readonly EventoAbordajeBC _eventoAbordajeBC;
        private readonly RutaEstudianteBC _rutaEstudianteBC;
        private readonly RecorridoBC _recorridoBC;
        private readonly ParaderoBC _paraderoBC;
        private readonly IncidenciaBC _incidenciaBC;

        public PadreFamiliaController(
            PadreFamiliaBC padreFamiliaBC,
            UsuarioBC usuarioBC,
            EstudianteBC estudianteBC,
            NotificacionBC notificacionBC,
            EventoAbordajeBC eventoAbordajeBC,
            RutaEstudianteBC rutaEstudianteBC,
            RecorridoBC recorridoBC,
            ParaderoBC paraderoBC,
            IncidenciaBC incidenciaBC)
        {
            _padreFamiliaBC = padreFamiliaBC;
            _usuarioBC = usuarioBC;
            _estudianteBC = estudianteBC;
            _notificacionBC = notificacionBC;
            _eventoAbordajeBC = eventoAbordajeBC;
            _rutaEstudianteBC = rutaEstudianteBC;
            _recorridoBC = recorridoBC;
            _paraderoBC = paraderoBC;
            _incidenciaBC = incidenciaBC;
        }

        public IActionResult Index()
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Padres);
            if (acceso != null)
                return acceso;

            PadreFamiliaBE? padre = ObtenerPadreAutenticado();
            if (padre == null)
            {
                TempData["error"] = "No existe un padre de familia vinculado al usuario autenticado.";
                return RedirectToAction("SesionInvalida", "Auth");
            }

            List<EstudianteBE> hijos = ObtenerHijos(padre.IdPadre);
            HashSet<Guid> idsHijos = hijos.Select(h => h.IdEstudiante).ToHashSet();

            PadreDashboardViewModel vm = new()
            {
                NombrePadre = padre.NombreCompleto,
                CantidadHijos = hijos.Count,
                CantidadNotificacionesPendientes = _notificacionBC.ListarNoLeidasPorPadre(padre.IdPadre).Count,
                CantidadEventosRecientes = _eventoAbordajeBC.Listar()
                    .Count(e => idsHijos.Contains(e.IdEstudiante) && e.FechaHora >= DateTime.Today.AddDays(-7))
            };

            return View(vm);
        }

        [HttpGet]
        public IActionResult MisHijos()
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Padres);
            if (acceso != null)
                return acceso;

            PadreFamiliaBE? padre = ObtenerPadreAutenticado();
            List<EstudianteBE> hijos = padre == null ? new() : ObtenerHijos(padre.IdPadre);

            return View(hijos);
        }

        [HttpGet]
        public IActionResult RegistrarHijo()
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Padres);
            if (acceso != null)
                return acceso;

            return View(new PadreRegistrarEstudianteViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RegistrarHijo(PadreRegistrarEstudianteViewModel vm)
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Padres);
            if (acceso != null)
                return acceso;

            PadreFamiliaBE? padre = ObtenerPadreAutenticado();
            if (padre == null)
            {
                TempData["error"] = "No se encontró el padre autenticado para registrar el estudiante.";
                return RedirectToAction(nameof(MisHijos));
            }

            if (!ModelState.IsValid)
                return View(vm);

            try
            {
                EstudianteBE entidad = new()
                {
                    IdPadre = padre.IdPadre,
                    Nombres = vm.Nombres,
                    ApellidoPaterno = vm.ApellidoPaterno,
                    ApellidoMaterno = vm.ApellidoMaterno,
                    DNI = vm.DNI,
                    FechaNacimiento = vm.FechaNacimiento,
                    Genero = vm.Genero,
                    Grado = vm.Grado,
                    Seccion = vm.Seccion,
                    Direccion = vm.Direccion,
                    Observaciones = vm.Observaciones,
                    Estado = true
                };

                bool ok = _estudianteBC.Registrar(entidad);
                TempData[ok ? "ok" : "error"] = ok
                    ? $"Hijo registrado correctamente. Código: {entidad.CodigoEstudiante}"
                    : "No se pudo registrar al estudiante. Verifique los datos e intente nuevamente.";

                if (ok)
                    return RedirectToAction(nameof(MisHijos));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            return View(vm);
        }

        [HttpGet]
        public IActionResult DetalleHijo(Guid idEstudiante)
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Padres);
            if (acceso != null)
                return acceso;

            PadreFamiliaBE? padre = ObtenerPadreAutenticado();
            if (padre == null)
                return RedirectToAction(nameof(MisHijos));

            EstudianteBE? estudiante = _estudianteBC.Listar()
                .FirstOrDefault(e => e.IdEstudiante == idEstudiante && e.IdPadre == padre.IdPadre);

            if (estudiante == null)
            {
                TempData["error"] = "No tiene acceso al estudiante solicitado.";
                return RedirectToAction(nameof(MisHijos));
            }

            RutaEstudianteBE? ruta = _rutaEstudianteBC.Listar()
                .Where(re => re.IdEstudiante == estudiante.IdEstudiante && re.Estado)
                .OrderByDescending(re => re.FechaInicioVigencia)
                .FirstOrDefault();

            ParaderoBE? subida = ruta?.IdParaderoSubida.HasValue == true ? _paraderoBC.ListarPorId(ruta.IdParaderoSubida.Value) : null;
            ParaderoBE? bajada = ruta?.IdParaderoBajada.HasValue == true ? _paraderoBC.ListarPorId(ruta.IdParaderoBajada.Value) : null;

            List<EventoAbordajeBE> eventos = _eventoAbordajeBC.Listar()
                .Where(e => e.IdEstudiante == estudiante.IdEstudiante)
                .OrderByDescending(e => e.FechaHora)
                .Take(20)
                .ToList();

            PadreHijoDetalleViewModel vm = new()
            {
                Estudiante = estudiante,
                RutaAsignada = ruta,
                ParaderoSubida = subida,
                ParaderoBajada = bajada,
                EventosRecientes = eventos
            };

            return View(vm);
        }

        [HttpGet]
        public IActionResult Notificaciones()
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Padres);
            if (acceso != null)
                return acceso;

            PadreFamiliaBE? padre = ObtenerPadreAutenticado();
            List<NotificacionBE> notificaciones = padre == null
                ? new List<NotificacionBE>()
                : _notificacionBC.ListarPorPadre(padre.IdPadre)
                    .OrderByDescending(n => n.FechaEnvio)
                    .ToList();

            return View(notificaciones);
        }

        [HttpGet]
        public IActionResult DetalleNotificacion(Guid idNotificacion)
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Padres);
            if (acceso != null)
                return acceso;

            PadreFamiliaBE? padre = ObtenerPadreAutenticado();
            if (padre == null)
                return RedirectToAction(nameof(Notificaciones));

            NotificacionBE? notificacion = _notificacionBC.ListarPorId(idNotificacion);
            if (notificacion == null || notificacion.IdPadre != padre.IdPadre)
            {
                TempData["error"] = "No tiene acceso a la notificación solicitada.";
                return RedirectToAction(nameof(Notificaciones));
            }

            return View(notificacion);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarcarNotificacionLeida(Guid idNotificacion)
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Padres);
            if (acceso != null)
                return acceso;

            PadreFamiliaBE? padre = ObtenerPadreAutenticado();
            if (padre == null)
                return RedirectToAction(nameof(Notificaciones));

            NotificacionBE? notificacion = _notificacionBC.ListarPorId(idNotificacion);
            if (notificacion == null || notificacion.IdPadre != padre.IdPadre)
            {
                TempData["error"] = "No tiene permiso para actualizar esta notificación.";
                return RedirectToAction(nameof(Notificaciones));
            }

            bool ok = _notificacionBC.MarcarLeida(idNotificacion);
            TempData[ok ? "ok" : "error"] = ok ? "Notificación marcada como leída." : "No se pudo actualizar la notificación.";
            return RedirectToAction(nameof(Notificaciones));
        }

        [HttpGet]
        public IActionResult Seguimiento()
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Padres);
            if (acceso != null)
                return acceso;

            PadreFamiliaBE? padre = ObtenerPadreAutenticado();
            if (padre == null)
                return View(new List<PadreSeguimientoItemViewModel>());

            List<EstudianteBE> hijos = ObtenerHijos(padre.IdPadre);
            HashSet<Guid> idsHijos = hijos.Select(h => h.IdEstudiante).ToHashSet();
            Dictionary<Guid, EstudianteBE> hijosDict = hijos.ToDictionary(h => h.IdEstudiante, h => h);

            List<RutaEstudianteBE> rutas = _rutaEstudianteBC.Listar()
                .Where(r => r.Estado && idsHijos.Contains(r.IdEstudiante))
                .ToList();

            List<EventoAbordajeBE> eventos = _eventoAbordajeBC.Listar();
            List<RecorridoBE> recorridos = _recorridoBC.Listar();

            List<PadreSeguimientoItemViewModel> vm = rutas.Select(r =>
            {
                EstudianteBE estudiante = hijosDict[r.IdEstudiante];
                ParaderoBE? paradaSubida = r.IdParaderoSubida.HasValue ? _paraderoBC.ListarPorId(r.IdParaderoSubida.Value) : null;

                RecorridoBE? recorridoHoy = recorridos
                    .Where(x => x.IdRuta == r.IdRuta && x.Fecha.Date == DateTime.Today)
                    .OrderByDescending(x => x.Fecha)
                    .FirstOrDefault();

                EventoAbordajeBE? ultimoEvento = eventos
                    .Where(e => e.IdEstudiante == r.IdEstudiante)
                    .OrderByDescending(e => e.FechaHora)
                    .FirstOrDefault();

                return new PadreSeguimientoItemViewModel
                {
                    CodigoEstudiante = estudiante.CodigoEstudiante,
                    NombreEstudiante = estudiante.NombreCompleto,
                    Ruta = $"{r.Ruta?.CodigoRuta} - {r.Ruta?.Nombre}",
                    Paradero = paradaSubida?.Nombre,
                    RecorridoDeHoy = recorridoHoy?.CodigoRecorrido ?? "Sin recorrido asignado hoy",
                    Estado = recorridoHoy?.EstadoRecorrido ?? "Sin estado disponible",
                    UltimoEvento = ultimoEvento?.TipoEvento,
                    FechaUltimoEvento = ultimoEvento?.FechaHora
                };
            }).ToList();

            return View(vm);
        }

        [HttpGet]
        public IActionResult MapaEnVivo(Guid? idEstudiante)
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Padres);
            if (acceso != null)
                return acceso;

            PadreFamiliaBE? padre = ObtenerPadreAutenticado();
            if (padre == null)
                return View(new PadreMapaEnVivoViewModel());

            List<EstudianteBE> hijos = ObtenerHijos(padre.IdPadre);
            if (!hijos.Any())
                return View(new PadreMapaEnVivoViewModel { MensajeEstado = "No hay hijos asociados para rastreo en vivo." });

            EstudianteBE estudianteSeleccionado = idEstudiante.HasValue
                ? hijos.FirstOrDefault(h => h.IdEstudiante == idEstudiante.Value) ?? hijos.First()
                : hijos.First();

            RutaEstudianteBE? asignacion = _rutaEstudianteBC.Listar()
                .Where(r => r.Estado && r.IdEstudiante == estudianteSeleccionado.IdEstudiante)
                .OrderByDescending(r => r.FechaInicioVigencia)
                .FirstOrDefault();

            RecorridoBE? recorridoActivo = asignacion == null
                ? null
                : _recorridoBC.Listar()
                    .Where(r => r.IdRuta == asignacion.IdRuta && r.Fecha.Date == DateTime.Today)
                    .OrderByDescending(r => r.Fecha)
                    .FirstOrDefault();

            EventoAbordajeBE? ultimoEvento = _eventoAbordajeBC.Listar()
                .Where(e => e.IdEstudiante == estudianteSeleccionado.IdEstudiante)
                .OrderByDescending(e => e.FechaHora)
                .FirstOrDefault();

            ParaderoBE? proximaParada = asignacion?.IdParaderoBajada.HasValue == true
                ? _paraderoBC.ListarPorId(asignacion.IdParaderoBajada.Value)
                : null;

            string mensajeEstado = recorridoActivo == null
                ? "No existe recorrido activo en este momento."
                : string.Equals(recorridoActivo.EstadoRecorrido, "EN_CURSO", StringComparison.OrdinalIgnoreCase)
                    ? "¡El autobús está cerca!"
                    : "Recorrido programado. Revisa el ETA del abordaje.";

            PadreMapaEnVivoViewModel vm = new()
            {
                Estudiante = estudianteSeleccionado,
                HijosDisponibles = hijos,
                RecorridoActivo = recorridoActivo,
                RutaAsignada = asignacion,
                UltimoEvento = ultimoEvento,
                ProximaParada = proximaParada,
                Conductor = recorridoActivo?.Conductor?.NombreCompleto,
                Bus = recorridoActivo?.Bus?.Placa,
                EstadoSeguridad = ultimoEvento == null ? "Sin eventos recientes" : "Monitoreo activo",
                ETA = recorridoActivo?.HoraInicioProgramada?.ToString(@"hh\:mm"),
                MensajeEstado = mensajeEstado,
                TieneRastreoDisponible = recorridoActivo != null
            };

            return View(vm);
        }

        [HttpGet]
        public IActionResult Historial(DateTime? fechaInicio, DateTime? fechaFin)
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Padres);
            if (acceso != null)
                return acceso;

            PadreFamiliaBE? padre = ObtenerPadreAutenticado();
            if (padre == null)
                return View(new PadreHistorialViewModel());

            DateTime inicio = (fechaInicio ?? DateTime.Today.AddDays(-30)).Date;
            DateTime fin = (fechaFin ?? DateTime.Today).Date;

            if (inicio > fin)
            {
                DateTime tmp = inicio;
                inicio = fin;
                fin = tmp;
            }

            List<EstudianteBE> hijos = ObtenerHijos(padre.IdPadre);
            HashSet<Guid> idsHijos = hijos.Select(h => h.IdEstudiante).ToHashSet();

            Dictionary<Guid, EstudianteBE> hijosDict = hijos.ToDictionary(h => h.IdEstudiante, h => h);
            List<RutaEstudianteBE> rutas = _rutaEstudianteBC.Listar().Where(r => r.Estado && idsHijos.Contains(r.IdEstudiante)).ToList();
            List<RecorridoBE> recorridos = _recorridoBC.Listar()
                .Where(r => r.Fecha.Date >= inicio && r.Fecha.Date <= fin)
                .ToList();
            List<EventoAbordajeBE> eventos = _eventoAbordajeBC.Listar()
                .Where(e => e.FechaHora.Date >= inicio && e.FechaHora.Date <= fin && idsHijos.Contains(e.IdEstudiante))
                .OrderByDescending(e => e.FechaHora)
                .ToList();
            List<IncidenciaBE> incidencias = _incidenciaBC.ListarPorPadre(padre.IdPadre);

            List<PadreHistorialViajeItemViewModel> viajes = rutas
                .Select(r =>
                {
                    EstudianteBE hijo = hijosDict[r.IdEstudiante];
                    RecorridoBE? recorrido = recorridos
                        .Where(x => x.IdRuta == r.IdRuta)
                        .OrderByDescending(x => x.Fecha)
                        .FirstOrDefault();
                    EventoAbordajeBE? ultimoEvento = eventos.FirstOrDefault(x => x.IdEstudiante == r.IdEstudiante);

                    return new PadreHistorialViajeItemViewModel
                    {
                        IdEstudiante = r.IdEstudiante,
                        Estudiante = hijo.NombreCompleto,
                        Fecha = recorrido?.Fecha ?? ultimoEvento?.FechaHora.Date,
                        TipoRecorrido = recorrido?.EstadoRecorrido ?? "Programado",
                        Ruta = $"{r.Ruta?.CodigoRuta} - {r.Ruta?.Nombre}",
                        Hora = ultimoEvento?.FechaHora.ToString("HH:mm") ?? "--:--",
                        Estado = recorrido?.EstadoRecorrido ?? "Sin estado",
                        Conductor = recorrido?.Conductor?.NombreCompleto ?? "No disponible",
                        TieneIncidencia = incidencias
                            .Any(i => string.Equals(i.CodigoRecorrido, recorrido?.CodigoRecorrido, StringComparison.OrdinalIgnoreCase))
                    };
                })
                .OrderByDescending(v => v.Fecha)
                .ToList();

            PadreHistorialViewModel vm = new()
            {
                FechaInicio = inicio,
                FechaFin = fin,
                TotalViajes = viajes.Count,
                ViajesSeguros = viajes.Count(v => !v.TieneIncidencia),
                Viajes = viajes
            };

            return View(vm);
        }

        [HttpGet]
        public IActionResult MiRuta()
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Padres);
            if (acceso != null)
                return acceso;

            PadreFamiliaBE? padre = ObtenerPadreAutenticado();
            if (padre == null)
                return View(new List<PadreRutaInfoViewModel>());

            List<EstudianteBE> hijos = ObtenerHijos(padre.IdPadre);
            Dictionary<Guid, EstudianteBE> hijosDict = hijos.ToDictionary(h => h.IdEstudiante, h => h);

            List<RutaEstudianteBE> rutasHijos = _rutaEstudianteBC.Listar()
                .Where(re => re.Estado && hijosDict.ContainsKey(re.IdEstudiante))
                .ToList();

            List<PadreRutaInfoViewModel> vm = rutasHijos
                .Select(re =>
                {
                    RecorridoBE? recorrido = _recorridoBC.Listar()
                        .Where(r => r.IdRuta == re.IdRuta)
                        .OrderByDescending(r => r.Fecha)
                        .FirstOrDefault();

                    EstudianteBE hijo = hijosDict[re.IdEstudiante];

                    return new PadreRutaInfoViewModel
                    {
                        CodigoEstudiante = hijo.CodigoEstudiante,
                        NombreEstudiante = hijo.NombreCompleto,
                        CodigoRuta = re.Ruta?.CodigoRuta ?? string.Empty,
                        NombreRuta = re.Ruta?.Nombre ?? string.Empty,
                        CodigoRecorrido = recorrido?.CodigoRecorrido,
                        EstadoRecorrido = recorrido?.EstadoRecorrido,
                        PlacaBus = recorrido?.Bus?.Placa,
                        NombreConductor = recorrido?.Conductor?.NombreCompleto,
                        FechaRecorrido = recorrido?.Fecha
                    };
                })
                .OrderBy(x => x.NombreEstudiante)
                .ToList();

            return View(vm);
        }

        [HttpGet]
        public IActionResult MiParadero()
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Padres);
            if (acceso != null)
                return acceso;

            PadreFamiliaBE? padre = ObtenerPadreAutenticado();
            if (padre == null)
                return View(new List<PadreParaderoInfoViewModel>());

            List<EstudianteBE> hijos = ObtenerHijos(padre.IdPadre);
            Dictionary<Guid, EstudianteBE> hijosDict = hijos.ToDictionary(h => h.IdEstudiante, h => h);

            List<PadreParaderoInfoViewModel> vm = _rutaEstudianteBC.Listar()
                .Where(re => re.Estado && hijosDict.ContainsKey(re.IdEstudiante))
                .Select(re =>
                {
                    ParaderoBE? subida = re.IdParaderoSubida.HasValue ? _paraderoBC.ListarPorId(re.IdParaderoSubida.Value) : null;
                    ParaderoBE? bajada = re.IdParaderoBajada.HasValue ? _paraderoBC.ListarPorId(re.IdParaderoBajada.Value) : null;
                    EstudianteBE hijo = hijosDict[re.IdEstudiante];

                    return new PadreParaderoInfoViewModel
                    {
                        CodigoEstudiante = hijo.CodigoEstudiante,
                        NombreEstudiante = hijo.NombreCompleto,
                        ParaderoSubida = subida?.Nombre,
                        DireccionSubida = subida?.Direccion,
                        ParaderoBajada = bajada?.Nombre,
                        DireccionBajada = bajada?.Direccion
                    };
                })
                .OrderBy(x => x.NombreEstudiante)
                .ToList();

            return View(vm);
        }

        [HttpGet]
        public IActionResult MisEventos(Guid? idEstudiante)
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Padres);
            if (acceso != null)
                return acceso;

            PadreFamiliaBE? padre = ObtenerPadreAutenticado();
            if (padre == null)
                return View(new List<EventoAbordajeBE>());

            HashSet<Guid> idsHijos = ObtenerHijos(padre.IdPadre)
                .Select(e => e.IdEstudiante)
                .ToHashSet();

            List<EventoAbordajeBE> eventos = _eventoAbordajeBC.Listar()
                .Where(e => idsHijos.Contains(e.IdEstudiante))
                .OrderByDescending(e => e.FechaHora)
                .ToList();

            if (idEstudiante.HasValue)
                eventos = eventos.Where(e => e.IdEstudiante == idEstudiante.Value).ToList();

            return View(eventos);
        }

        [HttpGet]
        public IActionResult Incidencias()
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Padres);
            if (acceso != null)
                return acceso;

            PadreFamiliaBE? padre = ObtenerPadreAutenticado();
            if (padre == null)
                return View(new List<IncidenciaBE>());

            List<IncidenciaBE> incidencias = _incidenciaBC.ListarPorPadre(padre.IdPadre);
            return View(incidencias);
        }

        [HttpGet]
        public IActionResult Listar()
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Administracion);
            if (acceso != null) return acceso;

            var lista = _padreFamiliaBC.Listar();
            return View(lista);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Administracion);
            if (acceso != null) return acceso;

            PadreFamiliaFormViewModel vm = new()
            {
                Estado = true,
                Usuarios = ObtenerUsuariosDisponibles()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(PadreFamiliaFormViewModel vm)
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
                PadreFamiliaBE entidad = new()
                {
                    IdUsuario = vm.IdUsuario,
                    Nombres = vm.Nombres,
                    ApellidoPaterno = vm.ApellidoPaterno,
                    ApellidoMaterno = vm.ApellidoMaterno,
                    DNI = vm.DNI,
                    Telefono = vm.Telefono,
                    TelefonoAlterno = vm.TelefonoAlterno,
                    Direccion = vm.Direccion,
                    CorreoContacto = vm.CorreoContacto,
                    Estado = vm.Estado
                };

                bool ok = _padreFamiliaBC.Registrar(entidad);

                if (ok)
                {
                    TempData["ok"] = "Padre de familia registrado correctamente.";
                    return RedirectToAction(nameof(Listar));
                }

                const string mensajeError = "No se pudo registrar el padre de familia.";
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

            var entidad = _padreFamiliaBC.ListarPorId(id);

            if (entidad == null)
            {
                TempData["error"] = "Padre de familia no encontrado.";
                return RedirectToAction(nameof(Listar));
            }

            PadreFamiliaFormViewModel vm = new()
            {
                IdPadre = entidad.IdPadre,
                IdUsuario = entidad.IdUsuario,
                CodigoPadre = entidad.CodigoPadre,
                Nombres = entidad.Nombres,
                ApellidoPaterno = entidad.ApellidoPaterno,
                ApellidoMaterno = entidad.ApellidoMaterno,
                DNI = entidad.DNI,
                Telefono = entidad.Telefono,
                TelefonoAlterno = entidad.TelefonoAlterno,
                Direccion = entidad.Direccion,
                CorreoContacto = entidad.CorreoContacto,
                Estado = entidad.Estado,
                Usuarios = ObtenerUsuariosParaEdicion(entidad.IdUsuario)
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(PadreFamiliaFormViewModel vm)
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
                PadreFamiliaBE entidad = new()
                {
                    IdPadre = vm.IdPadre,
                    IdUsuario = vm.IdUsuario,
                    CodigoPadre = vm.CodigoPadre,
                    Nombres = vm.Nombres,
                    ApellidoPaterno = vm.ApellidoPaterno,
                    ApellidoMaterno = vm.ApellidoMaterno,
                    DNI = vm.DNI,
                    Telefono = vm.Telefono,
                    TelefonoAlterno = vm.TelefonoAlterno,
                    Direccion = vm.Direccion,
                    CorreoContacto = vm.CorreoContacto,
                    Estado = vm.Estado
                };

                bool ok = _padreFamiliaBC.Actualizar(entidad);

                TempData[ok ? "ok" : "error"] = ok
                    ? "Padre de familia actualizado correctamente."
                    : "No se pudo actualizar el padre de familia.";

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
                bool ok = _padreFamiliaBC.Eliminar(id);

                TempData[ok ? "ok" : "error"] = ok
                    ? "Padre de familia eliminado correctamente."
                    : "No se pudo eliminar el padre de familia.";
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }

            return RedirectToAction(nameof(Listar));
        }

        private PadreFamiliaBE? ObtenerPadreAutenticado()
        {
            string? usuarioId = HttpContext.Session.GetString("UsuarioId");
            if (!Guid.TryParse(usuarioId, out Guid idUsuario))
                return null;

            return _padreFamiliaBC.ObtenerPorIdUsuario(idUsuario);
        }

        private List<EstudianteBE> ObtenerHijos(Guid idPadre)
        {
            return _estudianteBC.Listar()
                .Where(e => e.IdPadre == idPadre)
                .OrderBy(e => e.ApellidoPaterno)
                .ThenBy(e => e.Nombres)
                .ToList();
        }

        private List<SelectListItem> ObtenerUsuariosDisponibles()
        {
            return _padreFamiliaBC.ListarUsuariosDisponibles()
                .Select(u => new SelectListItem
                {
                    Value = u.IdUsuario.ToString(),
                    Text = $"{u.Username} - {u.Correo}"
                })
                .ToList();
        }

        private List<SelectListItem> ObtenerUsuariosParaEdicion(Guid idUsuarioActual)
        {
            var disponibles = _padreFamiliaBC.ListarUsuariosDisponibles();
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
