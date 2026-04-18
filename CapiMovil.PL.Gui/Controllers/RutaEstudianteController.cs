using CapiMovil.BL.BC;
using CapiMovil.BL.BE;
using CapiMovil.DL.DALC;
using CapiMovil.PL.Gui.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CapiMovil.PL.Gui.Controllers
{
    public class RutaEstudianteController : Controller
    {
        private readonly RutaEstudianteBC _rutaEstudianteBC;
        private readonly RutaDALC _rutaDALC;
        private readonly EstudianteDALC _estudianteDALC;
        private readonly ParaderoDALC _paraderoDALC;

        public RutaEstudianteController(
            RutaEstudianteBC rutaEstudianteBC,
            RutaDALC rutaDALC,
            EstudianteDALC estudianteDALC,
            ParaderoDALC paraderoDALC)
        {
            _rutaEstudianteBC = rutaEstudianteBC;
            _rutaDALC = rutaDALC;
            _estudianteDALC = estudianteDALC;
            _paraderoDALC = paraderoDALC;
        }

        public IActionResult Listar()
        {
            var lista = _rutaEstudianteBC.Listar();
            return View(lista);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            RutaEstudianteFormViewModel vm = new()
            {
                Rutas = ObtenerRutas(),
                Estudiantes = ObtenerEstudiantes(),
                ParaderosSubida = new List<SelectListItem>(),
                ParaderosBajada = new List<SelectListItem>(),
                EstadosAsignacion = ObtenerEstadosAsignacion(),
                FechaInicioVigencia = DateTime.Today,
                Estado = true,
                EstadoAsignacion = "ACTIVO"
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(RutaEstudianteFormViewModel vm)
        {
            if (vm.IdRuta == Guid.Empty)
                ModelState.AddModelError(nameof(vm.IdRuta), "Debe seleccionar una ruta.");

            if (vm.IdEstudiante == Guid.Empty)
                ModelState.AddModelError(nameof(vm.IdEstudiante), "Debe seleccionar un estudiante.");

            if (!ModelState.IsValid)
            {
                vm.Rutas = ObtenerRutas();
                vm.Estudiantes = ObtenerEstudiantes();
                vm.ParaderosSubida = vm.IdRuta != Guid.Empty ? ObtenerParaderos(vm.IdRuta) : new();
                vm.ParaderosBajada = vm.IdRuta != Guid.Empty ? ObtenerParaderos(vm.IdRuta) : new();
                vm.EstadosAsignacion = ObtenerEstadosAsignacion();
                return View(vm);
            }

            try
            {
                RutaEstudianteBE entidad = new()
                {
                    IdRuta = vm.IdRuta,
                    IdEstudiante = vm.IdEstudiante,
                    IdParaderoSubida = vm.IdParaderoSubida,
                    IdParaderoBajada = vm.IdParaderoBajada,
                    FechaInicioVigencia = vm.FechaInicioVigencia,
                    FechaFinVigencia = vm.FechaFinVigencia,
                    EstadoAsignacion = vm.EstadoAsignacion,
                    Observaciones = vm.Observaciones,
                    Estado = vm.Estado
                };

                bool ok = _rutaEstudianteBC.Registrar(entidad);

                TempData[ok ? "ok" : "error"] = ok
                    ? $"Asignación registrada correctamente. Código generado: {entidad.CodigoRutaEstudiante}"
                    : "No se pudo registrar la asignación.";

                if (ok)
                    return RedirectToAction(nameof(Listar));
            }
            catch (Exception ex)
            {
                ViewBag.SwalError = ex.Message;
            }

            vm.Rutas = ObtenerRutas();
            vm.Estudiantes = ObtenerEstudiantes();
            vm.ParaderosSubida = vm.IdRuta != Guid.Empty ? ObtenerParaderos(vm.IdRuta) : new();
            vm.ParaderosBajada = vm.IdRuta != Guid.Empty ? ObtenerParaderos(vm.IdRuta) : new();
            vm.EstadosAsignacion = ObtenerEstadosAsignacion();
            return View(vm);
        }

        [HttpGet]
        public IActionResult Editar(Guid id)
        {
            var entidad = _rutaEstudianteBC.ListarPorId(id);

            if (entidad == null)
            {
                TempData["error"] = "Asignación no encontrada.";
                return RedirectToAction(nameof(Listar));
            }

            RutaEstudianteFormViewModel vm = new()
            {
                IdRutaEstudiante = entidad.IdRutaEstudiante,
                IdRuta = entidad.IdRuta,
                IdEstudiante = entidad.IdEstudiante,
                IdParaderoSubida = entidad.IdParaderoSubida,
                IdParaderoBajada = entidad.IdParaderoBajada,
                CodigoRutaEstudiante = entidad.CodigoRutaEstudiante,
                FechaInicioVigencia = entidad.FechaInicioVigencia,
                FechaFinVigencia = entidad.FechaFinVigencia,
                EstadoAsignacion = entidad.EstadoAsignacion,
                Observaciones = entidad.Observaciones,
                Estado = entidad.Estado,
                Rutas = ObtenerRutas(),
                Estudiantes = ObtenerEstudiantes(),
                ParaderosSubida = ObtenerParaderos(entidad.IdRuta),
                ParaderosBajada = ObtenerParaderos(entidad.IdRuta),
                EstadosAsignacion = ObtenerEstadosAsignacion()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(RutaEstudianteFormViewModel vm)
        {
            if (vm.IdRuta == Guid.Empty)
                ModelState.AddModelError(nameof(vm.IdRuta), "Debe seleccionar una ruta.");

            if (vm.IdEstudiante == Guid.Empty)
                ModelState.AddModelError(nameof(vm.IdEstudiante), "Debe seleccionar un estudiante.");

            if (!ModelState.IsValid)
            {
                vm.Rutas = ObtenerRutas();
                vm.Estudiantes = ObtenerEstudiantes();
                vm.ParaderosSubida = vm.IdRuta != Guid.Empty ? ObtenerParaderos(vm.IdRuta) : new();
                vm.ParaderosBajada = vm.IdRuta != Guid.Empty ? ObtenerParaderos(vm.IdRuta) : new();
                vm.EstadosAsignacion = ObtenerEstadosAsignacion();
                return View(vm);
            }

            try
            {
                RutaEstudianteBE entidad = new()
                {
                    IdRutaEstudiante = vm.IdRutaEstudiante,
                    IdRuta = vm.IdRuta,
                    IdEstudiante = vm.IdEstudiante,
                    IdParaderoSubida = vm.IdParaderoSubida,
                    IdParaderoBajada = vm.IdParaderoBajada,
                    FechaInicioVigencia = vm.FechaInicioVigencia,
                    FechaFinVigencia = vm.FechaFinVigencia,
                    EstadoAsignacion = vm.EstadoAsignacion,
                    Observaciones = vm.Observaciones,
                    Estado = vm.Estado
                };

                bool ok = _rutaEstudianteBC.Actualizar(entidad);

                TempData[ok ? "ok" : "error"] = ok
                    ? "Asignación actualizada correctamente."
                    : "No se pudo actualizar la asignación.";

                if (ok)
                    return RedirectToAction(nameof(Listar));
            }
            catch (Exception ex)
            {
                ViewBag.SwalError = ex.Message;
            }

            vm.Rutas = ObtenerRutas();
            vm.Estudiantes = ObtenerEstudiantes();
            vm.ParaderosSubida = vm.IdRuta != Guid.Empty ? ObtenerParaderos(vm.IdRuta) : new();
            vm.ParaderosBajada = vm.IdRuta != Guid.Empty ? ObtenerParaderos(vm.IdRuta) : new();
            vm.EstadosAsignacion = ObtenerEstadosAsignacion();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(Guid id)
        {
            try
            {
                bool ok = _rutaEstudianteBC.Eliminar(id);

                TempData[ok ? "ok" : "error"] = ok
                    ? "Asignación eliminada correctamente."
                    : "No se pudo eliminar la asignación.";
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

        private List<SelectListItem> ObtenerEstudiantes()
        {
            return _estudianteDALC.ListarActivos()
                .Select(x => new SelectListItem
                {
                    Value = x.IdEstudiante.ToString(),
                    Text = $"{x.CodigoEstudiante} - {x.Nombres} {x.ApellidoPaterno} {x.ApellidoMaterno}"
                }).ToList();
        }

        private List<SelectListItem> ObtenerParaderos(Guid idRuta)
        {
            return _paraderoDALC.ListarPorRuta(idRuta)
                .Select(x => new SelectListItem
                {
                    Value = x.IdParadero.ToString(),
                    Text = $"{x.OrdenParada} - {x.Nombre}"
                }).ToList();
        }

        private List<SelectListItem> ObtenerEstadosAsignacion()
        {
            return new List<SelectListItem>
            {
                new("ACTIVO", "ACTIVO"),
                new("INACTIVO", "INACTIVO")
            };
        }
    }
}