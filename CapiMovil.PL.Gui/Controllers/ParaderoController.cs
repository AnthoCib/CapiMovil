using CapiMovil.BL.BC;
using CapiMovil.BL.BE;
using CapiMovil.DL.DALC;
using CapiMovil.PL.Gui.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CapiMovil.PL.Gui.Controllers
{
    public class ParaderoController : Controller
    {
        private readonly ParaderoBC _paraderoBC;
        private readonly RutaDALC _rutaDALC;

        public ParaderoController(ParaderoBC paraderoBC, RutaDALC rutaDALC)
        {
            _paraderoBC = paraderoBC;
            _rutaDALC = rutaDALC;
        }

        public IActionResult Listar()
        {
            var lista = _paraderoBC.Listar();
            return View(lista);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            ParaderoFormViewModel vm = new()
            {
                Rutas = ObtenerRutas(),
                Estado = true
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(ParaderoFormViewModel vm)
        {
            if (vm.IdRuta == Guid.Empty)
                ModelState.AddModelError(nameof(vm.IdRuta), "Debe seleccionar una ruta.");

            if (!ModelState.IsValid)
            {
                vm.Rutas = ObtenerRutas();
                return View(vm);
            }

            try
            {
                ParaderoBE entidad = new()
                {
                    IdRuta = vm.IdRuta,
                    Nombre = vm.Nombre,
                    Direccion = vm.Direccion,
                    Latitud = vm.Latitud,
                    Longitud = vm.Longitud,
                    OrdenParada = vm.OrdenParada,
                    HoraEstimada = vm.HoraEstimada,
                    Estado = vm.Estado
                };

                bool ok = _paraderoBC.Registrar(entidad);

                TempData[ok ? "ok" : "error"] = ok
                    ? $"Paradero registrado correctamente. Código generado: {entidad.CodigoParadero}"
                    : "No se pudo registrar el paradero.";

                if (ok)
                    return RedirectToAction(nameof(Listar));
            }
            catch (Exception ex)
            {
                ViewBag.SwalError = ex.Message;
            }

            vm.Rutas = ObtenerRutas();
            return View(vm);
        }

        [HttpGet]
        public IActionResult Editar(Guid id)
        {
            var entidad = _paraderoBC.ListarPorId(id);

            if (entidad == null)
            {
                TempData["error"] = "Paradero no encontrado.";
                return RedirectToAction(nameof(Listar));
            }

            ParaderoFormViewModel vm = new()
            {
                IdParadero = entidad.IdParadero,
                IdRuta = entidad.IdRuta,
                CodigoParadero = entidad.CodigoParadero,
                Nombre = entidad.Nombre,
                Direccion = entidad.Direccion,
                Latitud = entidad.Latitud,
                Longitud = entidad.Longitud,
                OrdenParada = entidad.OrdenParada,
                HoraEstimada = entidad.HoraEstimada,
                Estado = entidad.Estado,
                Rutas = ObtenerRutas()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(ParaderoFormViewModel vm)
        {
            if (vm.IdRuta == Guid.Empty)
                ModelState.AddModelError(nameof(vm.IdRuta), "Debe seleccionar una ruta.");

            if (!ModelState.IsValid)
            {
                vm.Rutas = ObtenerRutas();
                return View(vm);
            }

            try
            {
                ParaderoBE entidad = new()
                {
                    IdParadero = vm.IdParadero,
                    IdRuta = vm.IdRuta,
                    Nombre = vm.Nombre,
                    Direccion = vm.Direccion,
                    Latitud = vm.Latitud,
                    Longitud = vm.Longitud,
                    OrdenParada = vm.OrdenParada,
                    HoraEstimada = vm.HoraEstimada,
                    Estado = vm.Estado
                };

                bool ok = _paraderoBC.Actualizar(entidad);

                TempData[ok ? "ok" : "error"] = ok
                    ? "Paradero actualizado correctamente."
                    : "No se pudo actualizar el paradero.";

                if (ok)
                    return RedirectToAction(nameof(Listar));
            }
            catch (Exception ex)
            {
                ViewBag.SwalError = ex.Message;
            }

            vm.Rutas = ObtenerRutas();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(Guid id)
        {
            try
            {
                bool ok = _paraderoBC.Eliminar(id);

                TempData[ok ? "ok" : "error"] = ok
                    ? "Paradero eliminado correctamente."
                    : "No se pudo eliminar el paradero.";
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }

            return RedirectToAction(nameof(Listar));
        }

        private List<SelectListItem> ObtenerRutas()
        {
            return _rutaDALC.ListarActivas()
                .Select(x => new SelectListItem
                {
                    Value = x.IdRuta.ToString(),
                    Text = $"{x.CodigoRuta} - {x.Nombre}"
                })
                .ToList();
        }
    }
}