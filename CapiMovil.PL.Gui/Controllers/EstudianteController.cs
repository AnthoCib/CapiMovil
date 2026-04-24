using CapiMovil.BL.BC;
using CapiMovil.BL.BE;
using CapiMovil.PL.Gui.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CapiMovil.PL.Gui.Controllers
{
    public class EstudianteController : Controller
    {
        private readonly EstudianteBC _estudianteBC;
        private readonly PadreFamiliaBC _padreFamiliaBC;

        public EstudianteController(EstudianteBC estudianteBC, PadreFamiliaBC padreFamiliaBC)
        {
            _estudianteBC = estudianteBC;
            _padreFamiliaBC = padreFamiliaBC;
        }

        [HttpGet]
        public IActionResult Listar()
        {
            var lista = _estudianteBC.Listar();
            return View(lista);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            EstudianteFormViewModel vm = new EstudianteFormViewModel
            {
                Estado = true,
                Padres = ObtenerPadres(),
                Generos = ObtenerGeneros()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(EstudianteFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Padres = ObtenerPadres();
                vm.Generos = ObtenerGeneros();
                return View(vm);
            }

            try
            {
                EstudianteBE entidad = new EstudianteBE
                {
                    IdPadre = vm.IdPadre,
                    Nombres = vm.Nombres,
                    ApellidoPaterno = vm.ApellidoPaterno,
                    ApellidoMaterno = vm.ApellidoMaterno,
                    DNI = vm.DNI,
                    FechaNacimiento = vm.FechaNacimiento,
                    Genero = vm.Genero,
                    Grado = vm.Grado,
                    Seccion = vm.Seccion,
                    Direccion = vm.Direccion,
                    LatitudCasa = vm.LatitudCasa,
                    LongitudCasa = vm.LongitudCasa,
                    FotoUrl = vm.FotoUrl,
                    Observaciones = vm.Observaciones,
                    Estado = vm.Estado
                };

                bool ok = _estudianteBC.Registrar(entidad);

                if (ok)
                {
                    TempData["ok"] = "Estudiante registrado correctamente.";
                    return RedirectToAction(nameof(Listar));
                }

                const string mensajeError = "No se pudo registrar el estudiante.";
                ModelState.AddModelError(string.Empty, mensajeError);
                ViewBag.SwalError = mensajeError;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewBag.SwalError = ex.Message;
            }

            vm.Padres = ObtenerPadres();
            vm.Generos = ObtenerGeneros();
            return View(vm);
        }

        [HttpGet]
        public IActionResult Editar(Guid id)
        {
            var entidad = _estudianteBC.ListarPorId(id);

            if (entidad == null)
            {
                TempData["error"] = "Estudiante no encontrado.";
                return RedirectToAction(nameof(Listar));
            }

            EstudianteFormViewModel vm = new EstudianteFormViewModel
            {
                IdEstudiante = entidad.IdEstudiante,
                IdPadre = entidad.IdPadre,
                CodigoEstudiante = entidad.CodigoEstudiante,
                Nombres = entidad.Nombres,
                ApellidoPaterno = entidad.ApellidoPaterno,
                ApellidoMaterno = entidad.ApellidoMaterno,
                DNI = entidad.DNI,
                FechaNacimiento = entidad.FechaNacimiento,
                Genero = entidad.Genero,
                Grado = entidad.Grado,
                Seccion = entidad.Seccion,
                Direccion = entidad.Direccion,
                LatitudCasa = entidad.LatitudCasa,
                LongitudCasa = entidad.LongitudCasa,
                FotoUrl = entidad.FotoUrl,
                Observaciones = entidad.Observaciones,
                Estado = entidad.Estado,
                Padres = ObtenerPadres(),
                Generos = ObtenerGeneros()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(EstudianteFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Padres = ObtenerPadres();
                vm.Generos = ObtenerGeneros();
                return View(vm);
            }

            try
            {
                EstudianteBE entidad = new EstudianteBE
                {
                    IdEstudiante = vm.IdEstudiante,
                    IdPadre = vm.IdPadre,
                    Nombres = vm.Nombres,
                    ApellidoPaterno = vm.ApellidoPaterno,
                    ApellidoMaterno = vm.ApellidoMaterno,
                    DNI = vm.DNI,
                    FechaNacimiento = vm.FechaNacimiento,
                    Genero = vm.Genero,
                    Grado = vm.Grado,
                    Seccion = vm.Seccion,
                    Direccion = vm.Direccion,
                    LatitudCasa = vm.LatitudCasa,
                    LongitudCasa = vm.LongitudCasa,
                    FotoUrl = vm.FotoUrl,
                    Observaciones = vm.Observaciones,
                    Estado = vm.Estado
                };

                bool ok = _estudianteBC.Actualizar(entidad);

                TempData[ok ? "ok" : "error"] = ok
                    ? "Estudiante actualizado correctamente."
                    : "No se pudo actualizar el estudiante.";

                if (ok)
                    return RedirectToAction(nameof(Listar));
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }

            vm.Padres = ObtenerPadres();
            vm.Generos = ObtenerGeneros();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(Guid id)
        {
            try
            {
                bool ok = _estudianteBC.Eliminar(id);

                TempData[ok ? "ok" : "error"] = ok
                    ? "Estudiante eliminado correctamente."
                    : "No se pudo eliminar el estudiante.";
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }

            return RedirectToAction(nameof(Listar));
        }

        private List<SelectListItem> ObtenerPadres()
        {
            List<PadreFamiliaBE> padres = _padreFamiliaBC.ListarParaCombo();

            return padres
                .Select(p => new SelectListItem
                {
                    Value = p.IdPadre.ToString(),
                    Text = string.IsNullOrWhiteSpace(p.Usuario?.Username)
                        ? $"{p.CodigoPadre} - {p.NombreCompleto}"
                        : $"{p.CodigoPadre} - {p.NombreCompleto} ({p.Usuario?.Username})"
                })
                .ToList();
        }

        private List<SelectListItem> ObtenerGeneros()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "M", Text = "Masculino" },
                new SelectListItem { Value = "F", Text = "Femenino" }
            };
        }
    }
}
