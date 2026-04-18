using CapiMovil.BL.BC;
using CapiMovil.BL.BE;
using CapiMovil.DL.DALC;
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

        public EventoAbordajeController(
            EventoAbordajeBC eventoAbordajeBC,
            RecorridoDALC recorridoDALC,
            EstudianteDALC estudianteDALC,
            ParaderoDALC paraderoDALC)
        {
            _eventoAbordajeBC = eventoAbordajeBC;
            _recorridoDALC = recorridoDALC;
            _estudianteDALC = estudianteDALC;
            _paraderoDALC = paraderoDALC;
        }

        public IActionResult Listar()
        {
            var lista = _eventoAbordajeBC.Listar();
            return View(lista);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            EventoAbordajeFormViewModel vm = new()
            {
                Recorridos = ObtenerRecorridos(),
                Estudiantes = ObtenerEstudiantes(),
                Paraderos = ObtenerParaderos(),
                TiposEvento = ObtenerTiposEvento(),
                FechaHora = DateTime.Now,
                TipoEvento = "SUBIDA",
                Estado = true
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(EventoAbordajeFormViewModel vm)
        {
            if (vm.IdRecorrido == Guid.Empty)
                ModelState.AddModelError(nameof(vm.IdRecorrido), "Debe seleccionar un recorrido.");

            if (vm.IdEstudiante == Guid.Empty)
                ModelState.AddModelError(nameof(vm.IdEstudiante), "Debe seleccionar un estudiante.");

            if (!ModelState.IsValid)
            {
                vm.Recorridos = ObtenerRecorridos();
                vm.Estudiantes = ObtenerEstudiantes();
                vm.Paraderos = ObtenerParaderos();
                vm.TiposEvento = ObtenerTiposEvento();
                return View(vm);
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
                    FechaHora = vm.FechaHora,
                    Observacion = vm.Observacion,
                    Estado = vm.Estado
                };

                bool ok = _eventoAbordajeBC.Registrar(entidad);

                TempData[ok ? "ok" : "error"] = ok
                    ? $"Evento registrado correctamente. Código generado: {entidad.CodigoEvento}"
                    : "No se pudo registrar el evento.";

                if (ok)
                    return RedirectToAction(nameof(Listar));
            }
            catch (Exception ex)
            {
                ViewBag.SwalError = ex.Message;
            }

            vm.Recorridos = ObtenerRecorridos();
            vm.Estudiantes = ObtenerEstudiantes();
            vm.Paraderos = ObtenerParaderos();
            vm.TiposEvento = ObtenerTiposEvento();
            return View(vm);
        }

        [HttpGet]
        public IActionResult Editar(Guid id)
        {
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
    }
}