using CapiMovil.BL.BC;
using CapiMovil.BL.BE;
using CapiMovil.PL.Gui.Infrastructure;
using CapiMovil.PL.Gui.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CapiMovil.PL.Gui.Controllers
{
    public class IncidenciaController : Controller
    {
        private readonly IncidenciaBC _incidenciaBC;
        private readonly RecorridoBC _recorridoBC;
        private readonly ConductorBC _conductorBC;

        public IncidenciaController(
            IncidenciaBC incidenciaBC,
            RecorridoBC recorridoBC,
            ConductorBC conductorBC)
        {
            _incidenciaBC = incidenciaBC;
            _recorridoBC = recorridoBC;
            _conductorBC = conductorBC;
        }

        [HttpGet]
        public IActionResult Listar()
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Administracion);
            if (acceso != null) return acceso;

            List<IncidenciaBE> lista = _incidenciaBC.Listar();
            return View(lista);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Administracion);
            if (acceso != null) return acceso;

            IncidenciaFormViewModel vm = new IncidenciaFormViewModel
            {
                FechaHora = DateTime.Now,
                EstadoIncidencia = "PENDIENTE",
                Prioridad = "MEDIA"
            };

            CargarCombos(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(IncidenciaFormViewModel vm)
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Administracion);
            if (acceso != null) return acceso;

            if (!ModelState.IsValid)
            {
                CargarCombos(vm);
                return View(vm);
            }

            try
            {
                RecorridoBE? recorrido = _recorridoBC.ListarPorId(vm.IdRecorrido);

                if (recorrido == null)
                {
                    ModelState.AddModelError(string.Empty, "No se encontró el recorrido seleccionado.");
                    CargarCombos(vm);
                    return View(vm);
                }

                IncidenciaBE entidad = new IncidenciaBE
                {
                    IdRecorrido = vm.IdRecorrido,
                    IdConductor = recorrido.IdConductor,
                    ReportadoPor = vm.ReportadoPor,
                    TipoIncidencia = vm.TipoIncidencia,
                    Descripcion = vm.Descripcion,
                    FechaHora = vm.FechaHora,
                    EstadoIncidencia = vm.EstadoIncidencia,
                    Prioridad = vm.Prioridad,
                    Solucion = vm.Solucion
                };

                bool ok = _incidenciaBC.Registrar(entidad);

                TempData[ok ? "ok" : "error"] = ok
                    ? "Incidencia registrada correctamente."
                    : "No se pudo registrar la incidencia.";

                return RedirectToAction(nameof(Listar));
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Ocurrió un error al registrar la incidencia.");
            }

            CargarCombos(vm);
            return View(vm);
        }

        [HttpGet]
        public IActionResult Editar(Guid id)
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Administracion);
            if (acceso != null) return acceso;

            TempData["error"] = "La edición directa de incidencias está restringida. Use las acciones de seguimiento (detalle/cierre/estado).";
            return RedirectToAction(nameof(Listar));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(IncidenciaFormViewModel vm)
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Administracion);
            if (acceso != null) return acceso;

            TempData["error"] = "La edición directa de incidencias está restringida para administración.";
            return RedirectToAction(nameof(Listar));
        }

        [HttpGet]
        public IActionResult Detalle(Guid id)
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Administracion);
            if (acceso != null) return acceso;

            IncidenciaBE? entidad = _incidenciaBC.ListarPorId(id);

            if (entidad == null)
            {
                TempData["error"] = "La incidencia no existe.";
                return RedirectToAction(nameof(Listar));
            }

            return View(entidad);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(Guid id)
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Administracion);
            if (acceso != null) return acceso;

            TempData["error"] = "La eliminación de incidencias está deshabilitada para preservar la trazabilidad operativa.";

            return RedirectToAction(nameof(Listar));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Cerrar(Guid id, string solucion)
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Administracion);
            if (acceso != null) return acceso;

            try
            {
                bool ok = _incidenciaBC.Cerrar(id, solucion);

                TempData[ok ? "ok" : "error"] = ok
                    ? "Incidencia cerrada correctamente."
                    : "No se pudo cerrar la incidencia.";
            }
            catch (ArgumentException ex)
            {
                TempData["error"] = ex.Message;
            }
            catch (Exception)
            {
                TempData["error"] = "Ocurrió un error al cerrar la incidencia.";
            }

            return RedirectToAction(nameof(Listar));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CambiarEstado(Guid id, string estadoIncidencia)
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Administracion);
            if (acceso != null) return acceso;

            try
            {
                bool ok = _incidenciaBC.CambiarEstado(id, estadoIncidencia);

                TempData[ok ? "ok" : "error"] = ok
                    ? "Estado actualizado correctamente."
                    : "No se pudo actualizar el estado.";
            }
            catch (ArgumentException ex)
            {
                TempData["error"] = ex.Message;
            }
            catch (Exception)
            {
                TempData["error"] = "Ocurrió un error al cambiar el estado.";
            }

            return RedirectToAction(nameof(Listar));
        }

        [HttpGet]
        public JsonResult ObtenerConductorPorRecorrido(Guid idRecorrido)
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Administracion);
            if (acceso != null) return Json(new { ok = false, idConductor = "" });

            try
            {
                RecorridoBE? recorrido = _recorridoBC.ListarPorId(idRecorrido);

                if (recorrido == null || recorrido.IdConductor == Guid.Empty)
                {
                    return Json(new { ok = false, idConductor = "" });
                }

                return Json(new
                {
                    ok = true,
                    idConductor = recorrido.IdConductor.ToString()
                });
            }
            catch
            {
                return Json(new { ok = false, idConductor = "" });
            }
        }

        private void CargarCombos(IncidenciaFormViewModel vm)
        {
            vm.Recorridos = _recorridoBC.Listar()
                .Select(x => new SelectListItem
                {
                    Value = x.IdRecorrido.ToString(),
                    Text = x.CodigoRecorrido,
                    Selected = x.IdRecorrido == vm.IdRecorrido
                })
                .ToList();

            vm.Conductores = _conductorBC.Listar()
                .Select(x => new SelectListItem
                {
                    Value = x.IdConductor.ToString(),
                    Text = $"{x.CodigoConductor} - {x.Nombres} {x.ApellidoPaterno}",
                    Selected = x.IdConductor == vm.IdConductor
                })
                .ToList();

            vm.TiposIncidencia = new List<SelectListItem>
            {
                new SelectListItem { Value = "RETRASO", Text = "RETRASO", Selected = vm.TipoIncidencia == "RETRASO" },
                new SelectListItem { Value = "FALLA MECANICA", Text = "FALLA MECANICA", Selected = vm.TipoIncidencia == "FALLA MECANICA" },
                new SelectListItem { Value = "ACCIDENTE", Text = "ACCIDENTE", Selected = vm.TipoIncidencia == "ACCIDENTE" },
                new SelectListItem { Value = "DESVIO", Text = "DESVIO", Selected = vm.TipoIncidencia == "DESVIO" },
                new SelectListItem { Value = "AUSENCIA", Text = "AUSENCIA", Selected = vm.TipoIncidencia == "AUSENCIA" },
                new SelectListItem { Value = "OTRO", Text = "OTRO", Selected = vm.TipoIncidencia == "OTRO" }
            };

            vm.EstadosIncidencia = new List<SelectListItem>
            {
                new SelectListItem { Value = "PENDIENTE", Text = "PENDIENTE", Selected = vm.EstadoIncidencia == "PENDIENTE" },
                new SelectListItem { Value = "ATENDIDA", Text = "ATENDIDA", Selected = vm.EstadoIncidencia == "ATENDIDA" },
                new SelectListItem { Value = "CERRADA", Text = "CERRADA", Selected = vm.EstadoIncidencia == "CERRADA" }
            };

            vm.Prioridades = new List<SelectListItem>
            {
                new SelectListItem { Value = "BAJA", Text = "BAJA", Selected = vm.Prioridad == "BAJA" },
                new SelectListItem { Value = "MEDIA", Text = "MEDIA", Selected = vm.Prioridad == "MEDIA" },
                new SelectListItem { Value = "ALTA", Text = "ALTA", Selected = vm.Prioridad == "ALTA" },
                new SelectListItem { Value = "CRITICA", Text = "CRITICA", Selected = vm.Prioridad == "CRITICA" }
            };
        }
    }
}
