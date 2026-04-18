using Microsoft.AspNetCore.Mvc;

namespace CapiMovil.PL.Gui.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            string? usuarioId = HttpContext.Session.GetString("UsuarioId");

            if (string.IsNullOrEmpty(usuarioId))
                return RedirectToAction("Login", "Auth");

            return View();
        }
    }
}