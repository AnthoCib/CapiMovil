using CapiMovil.BL.BC;
using CapiMovil.BL.BE;
using CapiMovil.DL.DALC;
using CapiMovil.PL.Gui.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CapiMovil.PL.Gui.Controllers
{
    public class UbicacionBusController : Controller
    {
        private readonly UbicacionBusBC _ubicacionBusBC;
        private readonly RecorridoDALC _recorridoDALC;
        private readonly RecorridoBC _recorridoBC;

        public UbicacionBusController(
            UbicacionBusBC ubicacionBusBC,
            RecorridoDALC recorridoDALC,
            RecorridoBC recorridoBC)
        {
            _ubicacionBusBC = ubicacionBusBC;
            _recorridoDALC = recorridoDALC;
            _recorridoBC = recorridoBC;
        }

        public IActionResult Listar()
        {
            var lista = _ubicacionBusBC.Listar();
            return View(lista);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            UbicacionBusFormViewModel vm = new()
            {
                Recorridos = ObtenerRecorridos(),
                Fuentes = ObtenerFuentes(),
                FechaHora = DateTime.Now,
                Fuente = "MANUAL",
                Estado = true
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(UbicacionBusFormViewModel vm)
        {
            if (vm.IdRecorrido == Guid.Empty)
                ModelState.AddModelError(nameof(vm.IdRecorrido), "Debe seleccionar un recorrido.");

            if (!ModelState.IsValid)
            {
                vm.Recorridos = ObtenerRecorridos();
                vm.Fuentes = ObtenerFuentes();
                return View(vm);
            }

            try
            {
                UbicacionBusBE entidad = new()
                {
                    IdRecorrido = vm.IdRecorrido,
                    Latitud = vm.Latitud,
                    Longitud = vm.Longitud,
                    Velocidad = vm.Velocidad,
                    PrecisionMetros = vm.PrecisionMetros,
                    FechaHora = vm.FechaHora,
                    Fuente = vm.Fuente,
                    Estado = vm.Estado
                };

                bool ok = _ubicacionBusBC.Registrar(entidad);

                TempData[ok ? "ok" : "error"] = ok
                    ? $"Ubicación registrada correctamente. Código generado: {entidad.CodigoUbicacion}"
                    : "No se pudo registrar la ubicación.";

                if (ok)
                    return RedirectToAction(nameof(Listar));
            }
            catch (Exception ex)
            {
                ViewBag.SwalError = ex.Message;
            }

            vm.Recorridos = ObtenerRecorridos();
            vm.Fuentes = ObtenerFuentes();
            return View(vm);
        }

        [HttpGet]
        public IActionResult Editar(Guid id)
        {
            var entidad = _ubicacionBusBC.ListarPorId(id);

            if (entidad == null)
            {
                TempData["error"] = "Ubicación no encontrada.";
                return RedirectToAction(nameof(Listar));
            }

            UbicacionBusFormViewModel vm = new()
            {
                IdUbicacion = entidad.IdUbicacion,
                IdRecorrido = entidad.IdRecorrido,
                CodigoUbicacion = entidad.CodigoUbicacion,
                Latitud = entidad.Latitud,
                Longitud = entidad.Longitud,
                Velocidad = entidad.Velocidad,
                PrecisionMetros = entidad.PrecisionMetros,
                FechaHora = entidad.FechaHora,
                Fuente = entidad.Fuente,
                Estado = entidad.Estado,
                Recorridos = ObtenerRecorridosParaEdicion(entidad.IdRecorrido),
                Fuentes = ObtenerFuentes()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(UbicacionBusFormViewModel vm)
        {
            if (vm.IdRecorrido == Guid.Empty)
                ModelState.AddModelError(nameof(vm.IdRecorrido), "Debe seleccionar un recorrido.");

            if (!ModelState.IsValid)
            {
                vm.Recorridos = ObtenerRecorridosParaEdicion(vm.IdRecorrido);
                vm.Fuentes = ObtenerFuentes();
                return View(vm);
            }

            try
            {
                UbicacionBusBE entidad = new()
                {
                    IdUbicacion = vm.IdUbicacion,
                    IdRecorrido = vm.IdRecorrido,
                    Latitud = vm.Latitud,
                    Longitud = vm.Longitud,
                    Velocidad = vm.Velocidad,
                    PrecisionMetros = vm.PrecisionMetros,
                    FechaHora = vm.FechaHora,
                    Fuente = vm.Fuente,
                    Estado = vm.Estado
                };

                bool ok = _ubicacionBusBC.Actualizar(entidad);

                TempData[ok ? "ok" : "error"] = ok
                    ? "Ubicación actualizada correctamente."
                    : "No se pudo actualizar la ubicación.";

                if (ok)
                    return RedirectToAction(nameof(Listar));
            }
            catch (Exception ex)
            {
                ViewBag.SwalError = ex.Message;
            }

            vm.Recorridos = ObtenerRecorridosParaEdicion(vm.IdRecorrido);
            vm.Fuentes = ObtenerFuentes();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(Guid id)
        {
            try
            {
                bool ok = _ubicacionBusBC.Eliminar(id);

                TempData[ok ? "ok" : "error"] = ok
                    ? "Ubicación eliminada correctamente."
                    : "No se pudo eliminar la ubicación.";
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }

            return RedirectToAction(nameof(Listar));
        }

        private List<SelectListItem> ObtenerRecorridos()
        {
            return _recorridoDALC.ListarActivosParaOperacion()
                .Select(x => new SelectListItem
                {
                    Value = x.IdRecorrido.ToString(),
                    Text = $"{x.CodigoRecorrido} - {x.Fecha:dd/MM/yyyy}"
                }).ToList();
        }

        private List<SelectListItem> ObtenerRecorridosParaEdicion(Guid? idSeleccionado = null)
        {
            var lista = ObtenerRecorridos();

            if (idSeleccionado.HasValue && idSeleccionado.Value != Guid.Empty)
            {
                bool existe = lista.Any(x => x.Value == idSeleccionado.Value.ToString());

                if (!existe)
                {
                    var recorridoActual = _recorridoBC.ListarPorId(idSeleccionado.Value);

                    if (recorridoActual != null)
                    {
                        lista.Insert(0, new SelectListItem
                        {
                            Value = recorridoActual.IdRecorrido.ToString(),
                            Text = $"{recorridoActual.CodigoRecorrido} - {recorridoActual.Fecha:dd/MM/yyyy}"
                        });
                    }
                }
            }

            return lista;
        }
        private List<SelectListItem> ObtenerFuentes()
        {
            return new List<SelectListItem>
            {
                new("GPS", "GPS"),
                new("MANUAL", "MANUAL"),
                new("API", "API")
            };
        }
    }
}