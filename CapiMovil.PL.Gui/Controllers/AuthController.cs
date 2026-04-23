using CapiMovil.BL.BC;
using CapiMovil.BL.BE;
using CapiMovil.PL.Gui.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace CapiMovil.PL.Gui.Controllers
{
    public class AuthController : Controller
    {
        private readonly UsuarioBC _usuarioBC;

        public AuthController(UsuarioBC usuarioBC)
        {
            _usuarioBC = usuarioBC;
        }

        private IActionResult RedireccionarSegunRol(string? rol)
        {
            string rolNormalizado = (rol ?? "").Trim().ToUpperInvariant();

            return rolNormalizado switch
            {
                "ADMINISTRADOR" => RedirectToAction("Index", "Admin"),
                "ADMIN" => RedirectToAction("Index", "Admin"),
                "CONDUCTOR" => RedirectToAction("Index", "Conductor"),
                "PADRE" => RedirectToAction("Index", "PadreFamilia"),
                "PADRE DE FAMILIA" => RedirectToAction("Index", "PadreFamilia"),
                _ => RedirectToAction(nameof(SesionInvalida))
            };
        }

        [HttpGet]
        public IActionResult Login()
        {
            string? usuarioId = HttpContext.Session.GetString("UsuarioId");
            string? rol = HttpContext.Session.GetString("RolNombre");

            if (!string.IsNullOrEmpty(usuarioId))
            {
                return RedireccionarSegunRol(rol);
            }

            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            try
            {
                UsuarioBE? usuario = _usuarioBC.Login(vm.UsuarioOCorreo, vm.Password);

                if (usuario == null)
                {
                    ModelState.AddModelError(string.Empty, "Usuario/correo o contraseña incorrectos.");
                    return View(vm);
                }

                string rolNombre = (usuario.Rol?.Nombre ?? "").Trim().ToUpperInvariant();

                HttpContext.Session.SetString("UsuarioId", usuario.IdUsuario.ToString());
                HttpContext.Session.SetString("Username", usuario.Username ?? "");
                HttpContext.Session.SetString("Correo", usuario.Correo ?? "");
                HttpContext.Session.SetString("CodigoUsuario", usuario.CodigoUsuario ?? "");
                HttpContext.Session.SetString("RolId", usuario.Rol?.IdRol.ToString() ?? "");
                HttpContext.Session.SetString("RolNombre", rolNombre);

                TempData["ok"] = $"Bienvenido, {usuario.Username}. Rol: {rolNombre}";

                return RedireccionarSegunRol(rolNombre);
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Ocurrió un error al iniciar sesión: {ex.Message}");
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["ok"] = "Sesión cerrada correctamente.";
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult SesionInvalida()
        {
            HttpContext.Session.Clear();
            TempData["error"] = "La sesión no contiene un rol válido.";
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult AccesoDenegado()
        {
            return View();
        }
    }
}