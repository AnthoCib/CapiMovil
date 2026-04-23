using CapiMovil.BL.BC;
using CapiMovil.BL.BE;
using CapiMovil.PL.Gui.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data.SqlClient;

namespace CapiMovil.PL.Gui.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly UsuarioBC _usuarioBC;
        private readonly RolBC _rolBC;

        public UsuarioController(UsuarioBC usuarioBC, RolBC rolBC)
        {
            _usuarioBC = usuarioBC;
            _rolBC = rolBC;
        }

        [HttpGet]
        public IActionResult Listar()
        {
            var lista = _usuarioBC.Listar();
            return View(lista);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            UsuarioFormViewModel vm = new UsuarioFormViewModel
            {
                Estado = true,
                Roles = ObtenerRoles()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(UsuarioFormViewModel vm)
        {
            if (vm.IdRol == Guid.Empty)
                ModelState.AddModelError(nameof(vm.IdRol), "Debe seleccionar un rol.");

            if (string.IsNullOrWhiteSpace(vm.PasswordNueva))
                ModelState.AddModelError(nameof(vm.PasswordNueva), "La contraseña es obligatoria.");

            if (!ModelState.IsValid)
            {
                vm.Roles = ObtenerRoles();
                return View(vm);
            }

            try
            {
                UsuarioBE usuario = new UsuarioBE
                {
                    IdRol = vm.IdRol,
                    Username = vm.Username,
                    Correo = vm.Correo,
                    PasswordHash = vm.PasswordNueva,
                    Estado = vm.Estado
                };

                bool ok = _usuarioBC.Registrar(usuario);

                if (ok)
                {
                    TempData["ok"] = $"Usuario registrado correctamente.";
                    return RedirectToAction(nameof(Listar));
                }

                ModelState.AddModelError(string.Empty, "No se pudo registrar el usuario.");
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627 || ex.Number == 2601)
                {
                    if (ex.Message.Contains("UQ_Usuario_Correo"))
                        ModelState.AddModelError(nameof(vm.Correo), "El correo ya está registrado.");
                    else if (ex.Message.Contains("UQ_Usuario_Username"))
                        ModelState.AddModelError(nameof(vm.Username), "El nombre de usuario ya está registrado.");
                    else
                        ModelState.AddModelError(string.Empty, "Ya existe un registro con esos datos.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Ocurrió un error al registrar el usuario.");
                }
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Ocurrió un error inesperado.");
            }

            vm.Roles = ObtenerRoles();
            return View(vm);
        }

        [HttpGet]
        public IActionResult Editar(Guid id)
        {
            var usuario = _usuarioBC.ListarPorId(id);

            if (usuario == null)
            {
                TempData["error"] = "Usuario no encontrado.";
                return RedirectToAction(nameof(Listar));
            }

            UsuarioFormViewModel vm = new UsuarioFormViewModel
            {
                IdUsuario = usuario.IdUsuario,
                IdRol = usuario.IdRol,
                CodigoUsuario = usuario.CodigoUsuario,
                Username = usuario.Username,
                Correo = usuario.Correo,
                Estado = usuario.Estado,
                Roles = ObtenerRoles()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(UsuarioFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                var errores = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                ViewBag.SwalError = string.Join(" | ", errores);
                vm.Roles = ObtenerRoles();
                return View(vm);
            }

            try
            {
                UsuarioBE usuario = new UsuarioBE
                {
                    IdUsuario = vm.IdUsuario,
                    IdRol = vm.IdRol,
                    Username = vm.Username,
                    Correo = vm.Correo,
                    Estado = vm.Estado
                };

                bool ok = _usuarioBC.Actualizar(usuario);

                if (ok)
                {
                    TempData["ok"] = "Usuario actualizado correctamente.";
                    return RedirectToAction(nameof(Listar));
                }

                TempData["error"] = "No se pudo actualizar el usuario.";
            }
            catch (Exception ex)
            {

                ViewBag.SwalError = ex.Message;
            }

            ViewBag.SwalError = $"Id={vm.IdUsuario} | Rol={vm.IdRol} | User={vm.Username} | Correo={vm.Correo}";
            vm.Roles = ObtenerRoles();
            return View(vm);
        }


        [HttpGet]
        public IActionResult MiPerfil()
        {
            string? usuarioIdSession = HttpContext.Session.GetString("UsuarioId");

            if (string.IsNullOrEmpty(usuarioIdSession))
                return RedirectToAction("Login", "Auth");

            Guid idUsuario = Guid.Parse(usuarioIdSession);
            UsuarioBE? usuario = _usuarioBC.ListarPorId(idUsuario);

            if (usuario == null)
            {
                TempData["error"] = "Usuario no encontrado.";
                return RedirectToAction("Login", "Auth");
            }

            return View(usuario);
        }

        [HttpGet]
        public IActionResult CambiarPassword()
        {
            string? usuarioIdSession = HttpContext.Session.GetString("UsuarioId");

            if (string.IsNullOrEmpty(usuarioIdSession))
                return RedirectToAction("Login", "Auth");

            Guid idUsuario = Guid.Parse(usuarioIdSession);

            var usuario = _usuarioBC.ListarPorId(idUsuario);

            if (usuario == null)
            {
                TempData["error"] = "Usuario no encontrado.";
                return RedirectToAction("Login", "Auth");
            }

            UsuarioPasswordViewModel vm = new UsuarioPasswordViewModel
            {
                IdUsuario = usuario.IdUsuario,
                CodigoUsuario = usuario.CodigoUsuario,
                Username = usuario.Username
            };

            return View(vm);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CambiarPassword(UsuarioPasswordViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            try
            {
                string? usuarioIdSession = HttpContext.Session.GetString("UsuarioId");

                if (string.IsNullOrEmpty(usuarioIdSession))
                    return RedirectToAction("Login", "Auth");

                Guid idUsuarioLogueado = Guid.Parse(usuarioIdSession);

                bool ok = _usuarioBC.CambiarPassword(idUsuarioLogueado, vm.PasswordNueva, vm.ConfirmarPassword);

                if (ok)
                {
                    TempData["ok"] = "Contraseña actualizada correctamente.";
                    return RedirigirSegunRol();
                }

                ModelState.AddModelError(string.Empty, "No se pudo actualizar la contraseña.");
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Ocurrió un error al cambiar la contraseña.");
            }

            return View(vm);
        }
        private IActionResult RedirigirSegunRol()
        {
            if (User.IsInRole("ADMIN"))
                return RedirectToAction("Index", "Admin");

            if (User.IsInRole("CONDUCTOR"))
                return RedirectToAction("Index", "Conductor");

            if (User.IsInRole("PADRE"))
                return RedirectToAction("Index", "PadreFamilia");

            return RedirectToAction("Index", "Home");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(Guid id)
        {
            try
            {
                bool ok = _usuarioBC.Eliminar(id);

                TempData[ok ? "ok" : "error"] = ok
                    ? "Usuario eliminado correctamente."
                    : "No se pudo eliminar el usuario.";
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }

            return RedirectToAction(nameof(Listar));
        }

        private List<SelectListItem> ObtenerRoles()
        {
            return _rolBC.Listar()
                .Select(r => new SelectListItem
                {
                    Value = r.IdRol.ToString(),
                    Text = r.Nombre
                })
                .ToList();
        }


    }
}