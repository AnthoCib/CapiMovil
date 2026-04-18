using Microsoft.AspNetCore.Mvc;

namespace CapiMovil.PL.Gui.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            string? usuarioId = HttpContext.Session.GetString("UsuarioId");
            string? rol = HttpContext.Session.GetString("RolNombre");

            if (string.IsNullOrEmpty(usuarioId))
                return RedirectToAction("Login", "Auth");

            if ((rol ?? "").Trim().ToUpperInvariant() != "ADMINISTRADOR")
                return RedirectToAction("AccesoDenegado", "Auth");

            return View();
        }
    }
}
