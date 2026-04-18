using CapiMovil.BL.BC;
using CapiMovil.BL.BE;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace CapiMovil.PL.Gui.Controllers
{
    public class RolController : Controller
    {
        private readonly RolBC _rolBC;

        public RolController(RolBC rolBC)
        {
            _rolBC = rolBC;
        }

        [HttpGet]
        public IActionResult Listar()
        {
            var lista = _rolBC.Listar();
            return View(lista);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            RolBE rol = new RolBE
            {
                Estado = true
            };

            return View(rol);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(RolBE rol)
        {
            if (!ModelState.IsValid)
                return View(rol);

            try
            {
                bool ok = _rolBC.Registrar(rol);

                if (ok)
                {
                    TempData["ok"] = "Rol registrado correctamente.";
                    return RedirectToAction(nameof(Listar));
                }

                ModelState.AddModelError(string.Empty, "No se pudo registrar el rol.");
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627 || ex.Number == 2601)
                    ModelState.AddModelError(nameof(rol.Nombre), "El nombre del rol ya está registrado.");
                else
                    ModelState.AddModelError(string.Empty, "Ocurrió un error al registrar el rol.");
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Ocurrió un error inesperado.");
            }

            return View(rol);
        }

        [HttpGet]
        public IActionResult Editar(Guid id)
        {
            var rol = _rolBC.ListarPorId(id);

            if (rol == null)
            {
                TempData["error"] = "Rol no encontrado.";
                return RedirectToAction(nameof(Listar));
            }

            return View(rol);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(RolBE rol)
        {
            if (!ModelState.IsValid)
                return View(rol);

            try
            {
                bool ok = _rolBC.Actualizar(rol);

                if (ok)
                {
                    TempData["ok"] = "Rol actualizado correctamente.";
                    return RedirectToAction(nameof(Listar));
                }

                ModelState.AddModelError(string.Empty, "No se pudo actualizar el rol.");
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627 || ex.Number == 2601)
                    ModelState.AddModelError(nameof(rol.Nombre), "El nombre del rol ya está registrado.");
                else
                    ModelState.AddModelError(string.Empty, "Ocurrió un error al actualizar el rol.");
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Ocurrió un error inesperado.");
            }

            return View(rol);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(Guid id)
        {
            try
            {
                bool ok = _rolBC.Eliminar(id);

                TempData[ok ? "ok" : "error"] = ok
                    ? "Rol eliminado correctamente."
                    : "No se pudo eliminar el rol.";
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }

            return RedirectToAction(nameof(Listar));
        }
    }
}