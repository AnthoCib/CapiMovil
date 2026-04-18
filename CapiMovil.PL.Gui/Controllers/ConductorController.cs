using CapiMovil.BL.BC;
using CapiMovil.BL.BE;
using CapiMovil.PL.Gui.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CapiMovil.PL.Gui.Controllers
{
    public class ConductorController : Controller
    {
        private readonly ConductorBC _conductorBC;
        private readonly UsuarioBC _usuarioBC;

        public ConductorController(ConductorBC conductorBC, UsuarioBC usuarioBC)
        {
            _conductorBC = conductorBC;
            _usuarioBC = usuarioBC;
        }

        public IActionResult Index()
        {
            string? usuarioId = HttpContext.Session.GetString("UsuarioId");
            string? rol = HttpContext.Session.GetString("RolNombre");

            if (string.IsNullOrEmpty(usuarioId))
                return RedirectToAction("Login", "Auth");

            if ((rol ?? "").Trim().ToUpperInvariant() != "CONDUCTOR")
                return RedirectToAction("AccesoDenegado", "Auth");

            return View();
        }
        [HttpGet]
        public IActionResult Listar()
        {
            var lista = _conductorBC.Listar();
            return View(lista);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            ConductorFormViewModel vm = new ConductorFormViewModel
            {
                Estado = true,
                Usuarios = ObtenerUsuariosDisponibles()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(ConductorFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Usuarios = ObtenerUsuarios();
                return View(vm);
            }

            if (vm.IdUsuario == Guid.Empty)
                ModelState.AddModelError(nameof(vm.IdUsuario), "Debe seleccionar un usuario.");
            try
            {
                ConductorBE entidad = new ConductorBE
                {
                    IdUsuario = vm.IdUsuario,
                    CodigoConductor = vm.CodigoConductor,
                    Nombres = vm.Nombres,
                    ApellidoPaterno = vm.ApellidoPaterno,
                    ApellidoMaterno = vm.ApellidoMaterno,
                    DNI = vm.DNI,
                    Licencia = vm.Licencia,
                    CategoriaLicencia = vm.CategoriaLicencia,
                    Telefono = vm.Telefono,
                    Direccion = vm.Direccion,
                    Estado = vm.Estado
                };

                bool ok = _conductorBC.Registrar(entidad);

                TempData[ok ? "ok" : "error"] = ok
                    ? "Conductor registrado correctamente."
                    : "No se pudo registrar el conductor.";

                if (ok)
                    return RedirectToAction(nameof(Listar));
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }

            vm.Usuarios = ObtenerUsuarios();
            return View(vm);
        }

        [HttpGet]
        public IActionResult Editar(Guid id)
        {
            var entidad = _conductorBC.ListarPorId(id);

            if (entidad == null)
            {
                TempData["error"] = "Conductor no encontrado.";
                return RedirectToAction(nameof(Listar));
            }

            ConductorFormViewModel vm = new ConductorFormViewModel
            {
                IdConductor = entidad.IdConductor,
                IdUsuario = entidad.IdUsuario,
                CodigoConductor = entidad.CodigoConductor,
                Nombres = entidad.Nombres,
                ApellidoPaterno = entidad.ApellidoPaterno,
                ApellidoMaterno = entidad.ApellidoMaterno,
                DNI = entidad.DNI,
                Licencia = entidad.Licencia,
                CategoriaLicencia = entidad.CategoriaLicencia,
                Telefono = entidad.Telefono,
                Direccion = entidad.Direccion,
                Estado = entidad.Estado,
                Usuarios = ObtenerUsuarios()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(ConductorFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Usuarios = ObtenerUsuarios();
                return View(vm);
            }

            try
            {
                ConductorBE entidad = new ConductorBE
                {
                    IdConductor = vm.IdConductor,
                    IdUsuario = vm.IdUsuario,
                    CodigoConductor = vm.CodigoConductor,
                    Nombres = vm.Nombres,
                    ApellidoPaterno = vm.ApellidoPaterno,
                    ApellidoMaterno = vm.ApellidoMaterno,
                    DNI = vm.DNI,
                    Licencia = vm.Licencia,
                    CategoriaLicencia = vm.CategoriaLicencia,
                    Telefono = vm.Telefono,
                    Direccion = vm.Direccion,
                    Estado = vm.Estado
                };

                bool ok = _conductorBC.Actualizar(entidad);

                TempData[ok ? "ok" : "error"] = ok
                    ? "Conductor actualizado correctamente."
                    : "No se pudo actualizar el conductor.";

                if (ok)
                    return RedirectToAction(nameof(Listar));
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }

            vm.Usuarios = ObtenerUsuarios();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(Guid id)
        {
            try
            {
                bool ok = _conductorBC.Eliminar(id);

                TempData[ok ? "ok" : "error"] = ok
                    ? "Conductor eliminado correctamente."
                    : "No se pudo eliminar el conductor.";
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }

            return RedirectToAction(nameof(Listar));
        }

        private List<SelectListItem> ObtenerUsuarios()
        {
            var usuarios = _usuarioBC.Listar();

            return usuarios
                .Select(u => new SelectListItem
                {
                    Value = u.IdUsuario.ToString(),
                    Text = $"{u.Username} - {u.Correo}"
                })
                .ToList();
        }

        // Este método obtiene solo los usuarios que no están asignados a ningún conductor,
        // para evitar conflictos al editar o crear un conductor nuevo.
        private List<SelectListItem> ObtenerUsuariosDisponibles()
        {
            return _conductorBC.ListarUsuariosDisponibles()
                .Select(u => new SelectListItem
                {
                    Value = u.IdUsuario.ToString(),
                    Text = $"{u.Username} - {u.Correo}"
                })
                .ToList();
        }
    }
}