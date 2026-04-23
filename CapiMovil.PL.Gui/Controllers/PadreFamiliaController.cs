using CapiMovil.BL.BC;
using CapiMovil.BL.BE;
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

        public PadreFamiliaController(
            PadreFamiliaBC padreFamiliaBC,
            UsuarioBC usuarioBC,
            EstudianteBC estudianteBC,
            NotificacionBC notificacionBC)
        {
            _padreFamiliaBC = padreFamiliaBC;
            _usuarioBC = usuarioBC;
            _estudianteBC = estudianteBC;
            _notificacionBC = notificacionBC;
        }

        public IActionResult Index()
        {
            string? usuarioId = HttpContext.Session.GetString("UsuarioId");
            string? rol = HttpContext.Session.GetString("RolNombre");

            if (string.IsNullOrEmpty(usuarioId))
                return RedirectToAction("Login", "Auth");

            string rolNormalizado = (rol ?? "").Trim().ToUpperInvariant();

            if (rolNormalizado != "PADRE" && rolNormalizado != "PADRE DE FAMILIA")
                return RedirectToAction("AccesoDenegado", "Auth");

            if (!Guid.TryParse(usuarioId, out Guid idUsuario))
                return RedirectToAction("Login", "Auth");

            PadreFamiliaBE? padre = _padreFamiliaBC.Listar().FirstOrDefault(p => p.IdUsuario == idUsuario);

            ViewBag.CantidadHijos = 0;
            ViewBag.CantidadNotificaciones = 0;
            ViewBag.PadreNombre = "Padre de Familia";

            if (padre != null)
            {
                ViewBag.PadreNombre = padre.NombreCompleto;
                ViewBag.CantidadHijos = _estudianteBC.Listar().Count(e => e.IdPadre == padre.IdPadre);
                ViewBag.CantidadNotificaciones = _notificacionBC.Listar().Count(n => n.IdPadre == padre.IdPadre && !n.Leido);
            }

            return View();
        }

        [HttpGet]
        public IActionResult Listar()
        {
            var lista = _padreFamiliaBC.Listar();
            return View(lista);
        }

        [HttpGet]
        public IActionResult Crear()
        {
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

                TempData[ok ? "ok" : "error"] = ok
                    ? "Padre de familia registrado correctamente."
                    : "No se pudo registrar el padre de familia.";

                if (ok)
                    return RedirectToAction(nameof(Listar));
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
