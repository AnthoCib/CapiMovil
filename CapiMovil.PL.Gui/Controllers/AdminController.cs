using CapiMovil.PL.Gui.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace CapiMovil.PL.Gui.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Administracion);
            if (acceso != null)
                return acceso;

            return View();
        }
    }
}
