using CapiMovil.BL.BC;
using CapiMovil.BL.BE;
using CapiMovil.DL.DALC;
using CapiMovil.PL.Gui.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CapiMovil.PL.Gui.Controllers
{
    public class RecorridoController : Controller
    {
        private readonly RecorridoBC _recorridoBC;
        private readonly RutaDALC _rutaDALC;
        private readonly BusDALC _busDALC;
        private readonly ConductorDALC _conductorDALC;

        public RecorridoController(
            RecorridoBC recorridoBC,
            RutaDALC rutaDALC,
            BusDALC busDALC,
            ConductorDALC conductorDALC)
        {
            _recorridoBC = recorridoBC;
            _rutaDALC = rutaDALC;
            _busDALC = busDALC;
            _conductorDALC = conductorDALC;
        }

        public IActionResult Listar()
        {
            var lista = _recorridoBC.Listar();
            return View(lista);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            RecorridoFormViewModel vm = new()
            {
                Rutas = ObtenerRutas(),
                Buses = ObtenerBuses(),
                Conductores = ObtenerConductores(),
                EstadosRecorrido = ObtenerEstadosRecorrido(),
                Fecha = DateTime.Today,
                EstadoRecorrido = "PROGRAMADO",
                Estado = true
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(RecorridoFormViewModel vm)
        {
            if (vm.IdRuta == Guid.Empty)
                ModelState.AddModelError(nameof(vm.IdRuta), "Debe seleccionar una ruta.");

            if (vm.IdBus == Guid.Empty)
                ModelState.AddModelError(nameof(vm.IdBus), "Debe seleccionar un bus.");

            if (vm.IdConductor == Guid.Empty)
                ModelState.AddModelError(nameof(vm.IdConductor), "Debe seleccionar un conductor.");

            if (!ModelState.IsValid)
            {
                vm.Rutas = ObtenerRutas();
                vm.Buses = ObtenerBuses();
                vm.Conductores = ObtenerConductores();
                vm.EstadosRecorrido = ObtenerEstadosRecorrido();
                return View(vm);
            }

            try
            {
                RecorridoBE entidad = new()
                {
                    IdRuta = vm.IdRuta,
                    IdBus = vm.IdBus,
                    IdConductor = vm.IdConductor,
                    Fecha = vm.Fecha,
                    HoraInicioProgramada = vm.HoraInicioProgramada,
                    HoraFinProgramada = vm.HoraFinProgramada,
                    EstadoRecorrido = vm.EstadoRecorrido,
                    Observaciones = vm.Observaciones,
                    Estado = vm.Estado
                };

                bool ok = _recorridoBC.Registrar(entidad);

                TempData[ok ? "ok" : "error"] = ok
                    ? $"Recorrido registrado correctamente. Código generado: {entidad.CodigoRecorrido}"
                    : "No se pudo registrar el recorrido.";

                if (ok)
                    return RedirectToAction(nameof(Listar));
            }
            catch (Exception ex)
            {
                ViewBag.SwalError = ex.Message;
            }

            vm.Rutas = ObtenerRutas();
            vm.Buses = ObtenerBuses();
            vm.Conductores = ObtenerConductores();
            vm.EstadosRecorrido = ObtenerEstadosRecorrido();
            return View(vm);
        }

        [HttpGet]
        public IActionResult Editar(Guid id)
        {
            var entidad = _recorridoBC.ListarPorId(id);

            if (entidad == null)
            {
                TempData["error"] = "Recorrido no encontrado.";
                return RedirectToAction(nameof(Listar));
            }

            RecorridoFormViewModel vm = new()
            {
                IdRecorrido = entidad.IdRecorrido,
                IdRuta = entidad.IdRuta,
                IdBus = entidad.IdBus,
                IdConductor = entidad.IdConductor,
                CodigoRecorrido = entidad.CodigoRecorrido,
                Fecha = entidad.Fecha,
                HoraInicioProgramada = entidad.HoraInicioProgramada,
                HoraFinProgramada = entidad.HoraFinProgramada,
                EstadoRecorrido = entidad.EstadoRecorrido,
                Observaciones = entidad.Observaciones,
                Estado = entidad.Estado,
                Rutas = ObtenerRutas(),
                Buses = ObtenerBuses(),
                Conductores = ObtenerConductores(),
                EstadosRecorrido = ObtenerEstadosRecorrido()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(RecorridoFormViewModel vm)
        {
            if (vm.IdRuta == Guid.Empty)
                ModelState.AddModelError(nameof(vm.IdRuta), "Debe seleccionar una ruta.");

            if (vm.IdBus == Guid.Empty)
                ModelState.AddModelError(nameof(vm.IdBus), "Debe seleccionar un bus.");

            if (vm.IdConductor == Guid.Empty)
                ModelState.AddModelError(nameof(vm.IdConductor), "Debe seleccionar un conductor.");

            if (!ModelState.IsValid)
            {
                vm.Rutas = ObtenerRutas();
                vm.Buses = ObtenerBuses();
                vm.Conductores = ObtenerConductores();
                vm.EstadosRecorrido = ObtenerEstadosRecorrido();
                return View(vm);
            }

            try
            {
                RecorridoBE entidad = new()
                {
                    IdRecorrido = vm.IdRecorrido,
                    IdRuta = vm.IdRuta,
                    IdBus = vm.IdBus,
                    IdConductor = vm.IdConductor,
                    Fecha = vm.Fecha,
                    HoraInicioProgramada = vm.HoraInicioProgramada,
                    HoraFinProgramada = vm.HoraFinProgramada,
                    EstadoRecorrido = vm.EstadoRecorrido,
                    Observaciones = vm.Observaciones,
                    Estado = vm.Estado
                };

                bool ok = _recorridoBC.Actualizar(entidad);

                TempData[ok ? "ok" : "error"] = ok
                    ? "Recorrido actualizado correctamente."
                    : "No se pudo actualizar el recorrido.";

                if (ok)
                    return RedirectToAction(nameof(Listar));
            }
            catch (Exception ex)
            {
                ViewBag.SwalError = ex.Message;
            }

            vm.Rutas = ObtenerRutas();
            vm.Buses = ObtenerBuses();
            vm.Conductores = ObtenerConductores();
            vm.EstadosRecorrido = ObtenerEstadosRecorrido();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(Guid id)
        {
            try
            {
                bool ok = _recorridoBC.Eliminar(id);

                TempData[ok ? "ok" : "error"] = ok
                    ? "Recorrido eliminado correctamente."
                    : "No se pudo eliminar el recorrido.";
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }

            return RedirectToAction(nameof(Listar));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Iniciar(Guid id)
        {
            try
            {
                bool ok = _recorridoBC.Iniciar(id);
                TempData[ok ? "ok" : "error"] = ok
                    ? "Recorrido iniciado correctamente."
                    : "No se pudo iniciar el recorrido.";
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }

            return RedirectToAction(nameof(Listar));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Finalizar(Guid id)
        {
            try
            {
                bool ok = _recorridoBC.Finalizar(id);
                TempData[ok ? "ok" : "error"] = ok
                    ? "Recorrido finalizado correctamente."
                    : "No se pudo finalizar el recorrido.";
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }

            return RedirectToAction(nameof(Listar));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Cancelar(Guid id)
        {
            try
            {
                bool ok = _recorridoBC.Cancelar(id);
                TempData[ok ? "ok" : "error"] = ok
                    ? "Recorrido cancelado correctamente."
                    : "No se pudo cancelar el recorrido.";
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
                }).ToList();
        }

        private List<SelectListItem> ObtenerBuses()
        {
            return _busDALC.ListarActivos()
                .Select(x => new SelectListItem
                {
                    Value = x.IdBus.ToString(),
                    Text = $"{x.CodigoBus} - {x.Placa}"
                }).ToList();
        }

        private List<SelectListItem> ObtenerConductores()
        {
            return _conductorDALC.ListarActivos()
                .Select(x => new SelectListItem
                {
                    Value = x.IdConductor.ToString(),
                    Text = $"{x.CodigoConductor} - {x.Nombres} {x.ApellidoPaterno}"
                }).ToList();
        }

        private List<SelectListItem> ObtenerEstadosRecorrido()
        {
            return new List<SelectListItem>
            {
                new("PROGRAMADO", "PROGRAMADO"),
                new("EN_CURSO", "EN_CURSO"),
                new("FINALIZADO", "FINALIZADO"),
                new("CANCELADO", "CANCELADO")
            };
        }
    }
}