using CapiMovil.BL.BC;
using Microsoft.AspNetCore.Mvc;

namespace CapiMovil.PL.Gui.Controllers
{
    public class AuditoriaController : Controller
    {
        private readonly AuditoriaBC _auditoriaBC;

        public AuditoriaController(AuditoriaBC auditoriaBC)
        {
            _auditoriaBC = auditoriaBC;
        }

        [HttpGet]
        public IActionResult Listar()
        {
            var lista = _auditoriaBC.Listar();
            return View(lista);
        }

        [HttpGet]
        public IActionResult Detalle(Guid id)
        {
            var entidad = _auditoriaBC.ListarPorId(id);

            if (entidad == null)
            {
                TempData["error"] = "El registro de auditoría no existe.";
                return RedirectToAction(nameof(Listar));
            }

            return View(entidad);
        }
    }
}