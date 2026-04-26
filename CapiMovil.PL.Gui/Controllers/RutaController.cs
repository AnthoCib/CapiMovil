using CapiMovil.BL.BC;
using CapiMovil.BL.BE;
using CapiMovil.PL.Gui.Infrastructure;
using CapiMovil.PL.Gui.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CapiMovil.PL.Gui.Controllers
{
    public class RutaController : Controller
    {
        private readonly RutaBC _rutaBC;

        public RutaController(RutaBC rutaBC)
        {
            _rutaBC = rutaBC;
        }

        public IActionResult Listar()
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Administracion);
            if (acceso != null) return acceso;

            var lista = _rutaBC.Listar();
            return View(lista);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Administracion);
            if (acceso != null) return acceso;

            RutaFormViewModel vm = new()
            {
                Turnos = ObtenerTurnos(),
                EstadosRuta = ObtenerEstadosRuta(),
                Estado = true,
                Turno = "MANANA",
                EstadoRuta = "ACTIVA"
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(RutaFormViewModel vm)
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Administracion);
            if (acceso != null) return acceso;

            if (!ModelState.IsValid)
            {
                vm.Turnos = ObtenerTurnos();
                vm.EstadosRuta = ObtenerEstadosRuta();
                return View(vm);
            }

            try
            {
                RutaBE entidad = new()
                {
                    Nombre = vm.Nombre,
                    Descripcion = vm.Descripcion,
                    Turno = vm.Turno,
                    HoraInicio = vm.HoraInicio,
                    HoraFin = vm.HoraFin,
                    PuntoInicio = vm.PuntoInicio,
                    PuntoFin = vm.PuntoFin,
                    LatitudInicio = vm.LatitudInicio,
                    LongitudInicio = vm.LongitudInicio,
                    DireccionInicio = vm.DireccionInicio,
                    LatitudFin = vm.LatitudFin,
                    LongitudFin = vm.LongitudFin,
                    DireccionFin = vm.DireccionFin,
                    EstadoRuta = vm.EstadoRuta,
                    Estado = vm.Estado
                };

                bool ok = _rutaBC.Registrar(entidad);

                TempData[ok ? "ok" : "error"] = ok
                    ? $"Ruta registrada correctamente. Código generado: {entidad.CodigoRuta}"
                    : "No se pudo registrar la ruta.";

                if (ok)
                    return RedirectToAction(nameof(Listar));
            }
            catch (Exception ex)
            {
                ViewBag.SwalError = ex.Message;
            }

            vm.Turnos = ObtenerTurnos();
            vm.EstadosRuta = ObtenerEstadosRuta();
            return View(vm);
        }

        [HttpGet]
        public IActionResult Editar(Guid id)
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Administracion);
            if (acceso != null) return acceso;

            var ruta = _rutaBC.ListarPorId(id);

            if (ruta == null)
            {
                TempData["error"] = "Ruta no encontrada.";
                return RedirectToAction(nameof(Listar));
            }

            RutaFormViewModel vm = new()
            {
                IdRuta = ruta.IdRuta,
                CodigoRuta = ruta.CodigoRuta,
                Nombre = ruta.Nombre,
                Descripcion = ruta.Descripcion,
                Turno = ruta.Turno,
                HoraInicio = ruta.HoraInicio,
                HoraFin = ruta.HoraFin,
                PuntoInicio = ruta.PuntoInicio,
                PuntoFin = ruta.PuntoFin,
                LatitudInicio = ruta.LatitudInicio,
                LongitudInicio = ruta.LongitudInicio,
                DireccionInicio = ruta.DireccionInicio,
                LatitudFin = ruta.LatitudFin,
                LongitudFin = ruta.LongitudFin,
                DireccionFin = ruta.DireccionFin,
                EstadoRuta = ruta.EstadoRuta,
                Estado = ruta.Estado,
                Turnos = ObtenerTurnos(),
                EstadosRuta = ObtenerEstadosRuta()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(RutaFormViewModel vm)
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Administracion);
            if (acceso != null) return acceso;

            if (!ModelState.IsValid)
            {
                vm.Turnos = ObtenerTurnos();
                vm.EstadosRuta = ObtenerEstadosRuta();
                return View(vm);
            }

            try
            {
                RutaBE entidad = new()
                {
                    IdRuta = vm.IdRuta,
                    Nombre = vm.Nombre,
                    Descripcion = vm.Descripcion,
                    Turno = vm.Turno,
                    HoraInicio = vm.HoraInicio,
                    HoraFin = vm.HoraFin,
                    PuntoInicio = vm.PuntoInicio,
                    PuntoFin = vm.PuntoFin,
                    LatitudInicio = vm.LatitudInicio,
                    LongitudInicio = vm.LongitudInicio,
                    DireccionInicio = vm.DireccionInicio,
                    LatitudFin = vm.LatitudFin,
                    LongitudFin = vm.LongitudFin,
                    DireccionFin = vm.DireccionFin,
                    EstadoRuta = vm.EstadoRuta,
                    Estado = vm.Estado
                };

                bool ok = _rutaBC.Actualizar(entidad);

                TempData[ok ? "ok" : "error"] = ok
                    ? "Ruta actualizada correctamente."
                    : "No se pudo actualizar la ruta.";

                if (ok)
                    return RedirectToAction(nameof(Listar));
            }
            catch (Exception ex)
            {
                ViewBag.SwalError = ex.Message;
            }

            vm.Turnos = ObtenerTurnos();
            vm.EstadosRuta = ObtenerEstadosRuta();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(Guid id)
        {
            IActionResult? acceso = AutenticacionSesion.ValidarSesionYRol(this, RolesSistema.Administracion);
            if (acceso != null) return acceso;

            try
            {
                bool ok = _rutaBC.Eliminar(id);

                TempData[ok ? "ok" : "error"] = ok
                    ? "Ruta eliminada correctamente."
                    : "No se pudo eliminar la ruta.";
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }

            return RedirectToAction(nameof(Listar));
        }

        private List<SelectListItem> ObtenerTurnos()
        {
            return new List<SelectListItem>
            {
                new("MANANA", "MANANA"),
                new("TARDE", "TARDE"),
                new("NOCHE", "NOCHE")
            };
        }

        private List<SelectListItem> ObtenerEstadosRuta()
        {
            return new List<SelectListItem>
            {
                new("ACTIVA", "ACTIVA"),
                new("INACTIVA", "INACTIVA"),
                new("SUSPENDIDA", "SUSPENDIDA")
            };
        }
    }
}