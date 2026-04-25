using CapiMovil.BL.BC;
using CapiMovil.BL.BE;
using CapiMovil.PL.Gui.Infrastructure;
using CapiMovil.PL.Gui.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CapiMovil.PL.Gui.Controllers
{
    public class NotificacionController : Controller
    {
        private readonly NotificacionBC _notificacionBC;
        private readonly PadreFamiliaBC _padreFamiliaBC;
        private readonly EstudianteBC _estudianteBC;

        public NotificacionController(
            NotificacionBC notificacionBC,
            PadreFamiliaBC padreFamiliaBC,
            EstudianteBC estudianteBC)
        {
            _notificacionBC = notificacionBC;
            _padreFamiliaBC = padreFamiliaBC;
            _estudianteBC = estudianteBC;
        }

        [HttpGet]
        public IActionResult Listar()
        {
            var lista = _notificacionBC.Listar();
            return View(lista);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            if (EsAdminActual())
            {
                TempData["error"] = "La creación de notificaciones desde Admin está restringida. Use el módulo solo para consulta y seguimiento.";
                return RedirectToAction(nameof(Listar));
            }

            var vm = new NotificacionFormViewModel();
            CargarCombos(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(NotificacionFormViewModel vm)
        {
            if (EsAdminActual())
            {
                TempData["error"] = "La creación de notificaciones desde Admin está restringida.";
                return RedirectToAction(nameof(Listar));
            }

            if (!ModelState.IsValid)
            {
                CargarCombos(vm);
                return View(vm);
            }

            try
            {
                NotificacionBE entidad = new NotificacionBE
                {
                    IdPadre = vm.IdPadre,
                    IdEstudiante = vm.IdEstudiante,
                    Titulo = vm.Titulo,
                    Mensaje = vm.Mensaje,
                    TipoNotificacion = vm.TipoNotificacion,
                    Canal = vm.Canal,
                    Leido = vm.Leido
                };

                bool ok = _notificacionBC.Registrar(entidad);

                TempData[ok ? "ok" : "error"] = ok
                    ? "Notificación registrada correctamente."
                    : "No se pudo registrar la notificación.";

                return RedirectToAction(nameof(Listar));
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Ocurrió un error al registrar la notificación.");
            }

            CargarCombos(vm);
            return View(vm);
        }

        [HttpGet]
        public IActionResult Editar(Guid id)
        {
            if (EsAdminActual())
            {
                TempData["error"] = "La edición de notificaciones está restringida para Admin.";
                return RedirectToAction(nameof(Listar));
            }

            var entidad = _notificacionBC.ListarPorId(id);

            if (entidad == null)
            {
                TempData["error"] = "La notificación no existe.";
                return RedirectToAction(nameof(Listar));
            }

            var vm = new NotificacionFormViewModel
            {
                IdNotificacion = entidad.IdNotificacion,
                CodigoNotificacion = entidad.CodigoNotificacion,
                IdPadre = entidad.IdPadre,
                IdEstudiante = entidad.IdEstudiante,
                Titulo = entidad.Titulo,
                Mensaje = entidad.Mensaje,
                TipoNotificacion = entidad.TipoNotificacion,
                Canal = entidad.Canal,
                Leido = entidad.Leido
            };

            CargarCombos(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(NotificacionFormViewModel vm)
        {
            if (EsAdminActual())
            {
                TempData["error"] = "La edición de notificaciones está restringida para Admin.";
                return RedirectToAction(nameof(Listar));
            }

            if (!ModelState.IsValid)
            {
                CargarCombos(vm);
                return View(vm);
            }

            try
            {
                NotificacionBE entidad = new NotificacionBE
                {
                    IdNotificacion = vm.IdNotificacion,
                    IdPadre = vm.IdPadre,
                    IdEstudiante = vm.IdEstudiante,
                    Titulo = vm.Titulo,
                    Mensaje = vm.Mensaje,
                    TipoNotificacion = vm.TipoNotificacion,
                    Canal = vm.Canal,
                    Leido = vm.Leido
                };

                bool ok = _notificacionBC.Actualizar(entidad);

                TempData[ok ? "ok" : "error"] = ok
                    ? "Notificación actualizada correctamente."
                    : "No se pudo actualizar la notificación.";

                return RedirectToAction(nameof(Listar));
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Ocurrió un error al actualizar la notificación.");
            }

            CargarCombos(vm);
            return View(vm);
        }

        [HttpGet]
        public IActionResult Detalle(Guid id)
        {
            var entidad = _notificacionBC.ListarPorId(id);

            if (entidad == null)
            {
                TempData["error"] = "La notificación no existe.";
                return RedirectToAction(nameof(Listar));
            }

            return View(entidad);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(Guid id)
        {
            if (EsAdminActual())
            {
                TempData["error"] = "La eliminación de notificaciones está restringida para Admin.";
                return RedirectToAction(nameof(Listar));
            }

            try
            {
                bool ok = _notificacionBC.Eliminar(id);
                TempData[ok ? "ok" : "error"] = ok
                    ? "Notificación eliminada correctamente."
                    : "No se pudo eliminar la notificación.";
            }
            catch (ArgumentException ex)
            {
                TempData["error"] = ex.Message;
            }
            catch
            {
                TempData["error"] = "Ocurrió un error al eliminar la notificación.";
            }

            return RedirectToAction(nameof(Listar));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarcarLeida(Guid id)
        {
            if (EsAdminActual())
            {
                TempData["error"] = "El marcado manual de notificaciones está restringido para Admin.";
                return RedirectToAction(nameof(Listar));
            }

            try
            {
                bool ok = _notificacionBC.MarcarLeida(id);
                TempData[ok ? "ok" : "error"] = ok
                    ? "Notificación marcada como leída."
                    : "No se pudo marcar la notificación.";
            }
            catch (ArgumentException ex)
            {
                TempData["error"] = ex.Message;
            }
            catch
            {
                TempData["error"] = "Ocurrió un error al marcar la notificación.";
            }

            return RedirectToAction(nameof(Listar));
        }

        private void CargarCombos(NotificacionFormViewModel vm)
        {
            vm.Padres = _padreFamiliaBC.Listar()
                .Select(x => new SelectListItem
                {
                    Value = x.IdPadre.ToString(),
                    Text = $"{x.CodigoPadre} - {x.Nombres} {x.ApellidoPaterno}",
                    Selected = x.IdPadre == vm.IdPadre
                }).ToList();

            vm.Estudiantes = _estudianteBC.Listar()
                .Select(x => new SelectListItem
                {
                    Value = x.IdEstudiante.ToString(),
                    Text = $"{x.CodigoEstudiante} - {x.Nombres} {x.ApellidoPaterno}",
                    Selected = x.IdEstudiante == vm.IdEstudiante
                }).ToList();

            vm.Tipos = new()
            {
                new SelectListItem { Value = "INFO", Text = "INFO", Selected = vm.TipoNotificacion == "INFO" },
                new SelectListItem { Value = "ALERTA", Text = "ALERTA", Selected = vm.TipoNotificacion == "ALERTA" },
                new SelectListItem { Value = "SUBIDA", Text = "SUBIDA", Selected = vm.TipoNotificacion == "SUBIDA" },
                new SelectListItem { Value = "BAJADA", Text = "BAJADA", Selected = vm.TipoNotificacion == "BAJADA" },
                new SelectListItem { Value = "RETRASO", Text = "RETRASO", Selected = vm.TipoNotificacion == "RETRASO" },
                new SelectListItem { Value = "INCIDENCIA", Text = "INCIDENCIA", Selected = vm.TipoNotificacion == "INCIDENCIA" }
            };

            vm.Canales = new()
            {
                new SelectListItem { Value = "SISTEMA", Text = "SISTEMA", Selected = vm.Canal == "SISTEMA" },
                new SelectListItem { Value = "EMAIL", Text = "EMAIL", Selected = vm.Canal == "EMAIL" },
                new SelectListItem { Value = "SMS", Text = "SMS", Selected = vm.Canal == "SMS" },
                new SelectListItem { Value = "PUSH", Text = "PUSH", Selected = vm.Canal == "PUSH" }
            };
        }

        private bool EsAdminActual()
        {
            string rol = HttpContext.Session.GetString("RolNombre") ?? string.Empty;
            string rolNormalizado = AutenticacionSesion.NormalizarRol(rol);
            return RolesSistema.Administracion.Contains(rolNormalizado);
        }
    }
}
