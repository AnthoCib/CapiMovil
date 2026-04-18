using CapiMovil.BL.BC;
using CapiMovil.BL.BE;
using CapiMovil.PL.Gui.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CapiMovil.PL.Gui.Controllers
{
    public class BusController : Controller
    {
        private readonly BusBC _busBC;

        public BusController(BusBC busBC)
        {
            _busBC = busBC;
        }

        public IActionResult Listar()
        {
            var lista = _busBC.Listar();
            return View(lista);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            BusFormViewModel vm = new()
            {
                EstadosOperacion = ObtenerEstadosOperacion(),
                Estado = true,
                SeguroVigente = true,
                EstadoOperacion = "ACTIVO"
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(BusFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.EstadosOperacion = ObtenerEstadosOperacion();
                return View(vm);
            }

            try
            {
                BusBE entidad = new()
                {
                    Placa = vm.Placa,
                    Marca = vm.Marca,
                    Modelo = vm.Modelo,
                    Color = vm.Color,
                    Anio = vm.Anio,
                    Capacidad = vm.Capacidad,
                    EstadoOperacion = vm.EstadoOperacion,
                    SeguroVigente = vm.SeguroVigente,
                    FechaVencimientoSOAT = vm.FechaVencimientoSOAT,
                    FechaRevisionTecnica = vm.FechaRevisionTecnica,
                    Estado = vm.Estado
                };

                bool ok = _busBC.Registrar(entidad);

                TempData[ok ? "ok" : "error"] = ok
                    ? $"Bus registrado correctamente. Código generado: {entidad.CodigoBus}"
                    : "No se pudo registrar el bus.";

                if (ok)
                    return RedirectToAction(nameof(Listar));
            }
            catch (Exception ex)
            {
                ViewBag.SwalError = ex.Message;
            }

            vm.EstadosOperacion = ObtenerEstadosOperacion();
            return View(vm);
        }

        [HttpGet]
        public IActionResult Editar(Guid id)
        {
            var bus = _busBC.ListarPorId(id);

            if (bus == null)
            {
                TempData["error"] = "Bus no encontrado.";
                return RedirectToAction(nameof(Listar));
            }

            BusFormViewModel vm = new()
            {
                IdBus = bus.IdBus,
                CodigoBus = bus.CodigoBus,
                Placa = bus.Placa,
                Marca = bus.Marca,
                Modelo = bus.Modelo,
                Color = bus.Color,
                Anio = bus.Anio,
                Capacidad = bus.Capacidad,
                EstadoOperacion = bus.EstadoOperacion,
                SeguroVigente = bus.SeguroVigente,
                FechaVencimientoSOAT = bus.FechaVencimientoSOAT,
                FechaRevisionTecnica = bus.FechaRevisionTecnica,
                Estado = bus.Estado,
                EstadosOperacion = ObtenerEstadosOperacion()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(BusFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.EstadosOperacion = ObtenerEstadosOperacion();
                return View(vm);
            }

            try
            {
                BusBE entidad = new()
                {
                    IdBus = vm.IdBus,
                    Placa = vm.Placa,
                    Marca = vm.Marca,
                    Modelo = vm.Modelo,
                    Color = vm.Color,
                    Anio = vm.Anio,
                    Capacidad = vm.Capacidad,
                    EstadoOperacion = vm.EstadoOperacion,
                    SeguroVigente = vm.SeguroVigente,
                    FechaVencimientoSOAT = vm.FechaVencimientoSOAT,
                    FechaRevisionTecnica = vm.FechaRevisionTecnica,
                    Estado = vm.Estado
                };

                bool ok = _busBC.Actualizar(entidad);

                TempData[ok ? "ok" : "error"] = ok
                    ? "Bus actualizado correctamente."
                    : "No se pudo actualizar el bus.";

                if (ok)
                    return RedirectToAction(nameof(Listar));
            }
            catch (Exception ex)
            {
                ViewBag.SwalError = ex.Message;
            }

            vm.EstadosOperacion = ObtenerEstadosOperacion();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(Guid id)
        {
            try
            {
                bool ok = _busBC.Eliminar(id);

                TempData[ok ? "ok" : "error"] = ok
                    ? "Bus eliminado correctamente."
                    : "No se pudo eliminar el bus.";
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }

            return RedirectToAction(nameof(Listar));
        }

        private List<SelectListItem> ObtenerEstadosOperacion()
        {
            return new List<SelectListItem>
            {
                new("ACTIVO", "ACTIVO"),
                new("INACTIVO", "INACTIVO"),
                new("MANTENIMIENTO", "MANTENIMIENTO")
            };
        }
    }
}