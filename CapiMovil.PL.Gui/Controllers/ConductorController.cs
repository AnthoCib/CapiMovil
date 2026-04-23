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
        private readonly RecorridoBC _recorridoBC;
        private readonly IncidenciaBC _incidenciaBC;

        public ConductorController(
            ConductorBC conductorBC,
            UsuarioBC usuarioBC,
            RecorridoBC recorridoBC,
            IncidenciaBC incidenciaBC)
        {
            _conductorBC = conductorBC;
            _usuarioBC = usuarioBC;
            _recorridoBC = recorridoBC;
            _incidenciaBC = incidenciaBC;
        }

        public IActionResult Index()
        {
            IActionResult? acceso = ValidarSesionYRol("CONDUCTOR");
            if (acceso != null)
                return acceso;

            string usuarioId = HttpContext.Session.GetString("UsuarioId")!;
            Guid idUsuario = Guid.Parse(usuarioId);

            ConductorBE? conductor = _conductorBC.Listar().FirstOrDefault(c => c.IdUsuario == idUsuario);

            ViewBag.ConductorNombre = conductor?.NombreCompleto ?? "Conductor";
            ViewBag.RecorridosHoy = 0;
            ViewBag.IncidenciasAbiertas = 0;

            if (conductor != null)
            {
                ViewBag.RecorridosHoy = _recorridoBC.Listar()
                    .Count(r => r.IdConductor == conductor.IdConductor && r.Fecha.Date == DateTime.Today);
                ViewBag.IncidenciasAbiertas = _incidenciaBC.Listar()
                    .Count(i => i.IdConductor == conductor.IdConductor && i.EstadoIncidencia != "RESUELTA");
            }

            return View();
        }

        public IActionResult MiConductor()
        {
            IActionResult? acceso = ValidarSesionYRol("PADRE", "PADRE DE FAMILIA");
            if (acceso != null)
                return acceso;

            ConductorBE? conductor = _conductorBC.Listar().FirstOrDefault(c => c.Estado);

            if (conductor == null)
            {
                TempData["error"] = "No hay un conductor disponible para mostrar en este momento.";
                return RedirectToAction("Index", "PadreFamilia");
            }

            return View(conductor);
        }

        [HttpGet]
        public IActionResult MiBus()
        {
            IActionResult? acceso = ValidarSesionYRol("CONDUCTOR");
            if (acceso != null) return acceso;
            return View();
        }

        [HttpGet]
        public IActionResult MiRuta()
        {
            IActionResult? acceso = ValidarSesionYRol("CONDUCTOR");
            if (acceso != null) return acceso;
            return View();
        }

        [HttpGet]
        public IActionResult Estudiantes()
        {
            IActionResult? acceso = ValidarSesionYRol("CONDUCTOR");
            if (acceso != null) return acceso;
            return View();
        }

        [HttpGet]
        public IActionResult Recorrido()
        {
            IActionResult? acceso = ValidarSesionYRol("CONDUCTOR");
            if (acceso != null) return acceso;
            return View();
        }

        [HttpGet]
        public IActionResult Abordaje()
        {
            IActionResult? acceso = ValidarSesionYRol("CONDUCTOR");
            if (acceso != null) return acceso;
            return View();
        }

        [HttpGet]
        public IActionResult Incidencias()
        {
            IActionResult? acceso = ValidarSesionYRol("CONDUCTOR");
            if (acceso != null) return acceso;
            return View();
        }

        [HttpGet]
        public IActionResult Listar()
        {
            IActionResult? acceso = ValidarSesionYRol("ADMINISTRADOR");
            if (acceso != null) return acceso;

            var lista = _conductorBC.Listar();
            return View(lista);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            IActionResult? acceso = ValidarSesionYRol("ADMINISTRADOR");
            if (acceso != null) return acceso;

            ConductorFormViewModel vm = new()
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
            IActionResult? acceso = ValidarSesionYRol("ADMINISTRADOR");
            if (acceso != null) return acceso;

            if (vm.IdUsuario == Guid.Empty)
                ModelState.AddModelError(nameof(vm.IdUsuario), "Debe seleccionar un usuario.");

            if (!ModelState.IsValid)
            {
                vm.Usuarios = ObtenerUsuariosDisponibles();
                return View(vm);
            }

            try
            {
                ConductorBE entidad = new()
                {
                    IdUsuario = vm.IdUsuario,
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
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewBag.SwalError = ex.Message;
            }

            vm.Usuarios = ObtenerUsuariosDisponibles();
            return View(vm);
        }

        [HttpGet]
        public IActionResult Editar(Guid id)
        {
            IActionResult? acceso = ValidarSesionYRol("ADMINISTRADOR");
            if (acceso != null) return acceso;

            var entidad = _conductorBC.ListarPorId(id);

            if (entidad == null)
            {
                TempData["error"] = "Conductor no encontrado.";
                return RedirectToAction(nameof(Listar));
            }

            ConductorFormViewModel vm = new()
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
                Usuarios = ObtenerUsuariosParaEdicion(entidad.IdUsuario)
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(ConductorFormViewModel vm)
        {
            IActionResult? acceso = ValidarSesionYRol("ADMINISTRADOR");
            if (acceso != null) return acceso;

            if (vm.IdUsuario == Guid.Empty)
                ModelState.AddModelError(nameof(vm.IdUsuario), "Debe seleccionar un usuario.");

            if (!ModelState.IsValid)
            {
                vm.Usuarios = ObtenerUsuariosParaEdicion(vm.IdUsuario);
                return View(vm);
            }

            try
            {
                ConductorBE entidad = new()
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
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewBag.SwalError = ex.Message;
            }

            vm.Usuarios = ObtenerUsuariosParaEdicion(vm.IdUsuario);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(Guid id)
        {
            IActionResult? acceso = ValidarSesionYRol("ADMINISTRADOR");
            if (acceso != null) return acceso;

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

        private IActionResult? ValidarSesionYRol(params string[] rolesPermitidos)
        {
            string? usuarioId = HttpContext.Session.GetString("UsuarioId");
            string? rol = HttpContext.Session.GetString("RolNombre");

            if (string.IsNullOrWhiteSpace(usuarioId))
                return RedirectToAction("Login", "Auth");

            string rolNormalizado = (rol ?? string.Empty).Trim().ToUpperInvariant();

            bool permitido = rolesPermitidos
                .Select(r => r.Trim().ToUpperInvariant())
                .Contains(rolNormalizado);

            if (!permitido)
                return RedirectToAction("AccesoDenegado", "Auth");

            return null;
        }

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

        private List<SelectListItem> ObtenerUsuariosParaEdicion(Guid idUsuarioActual)
        {
            var disponibles = _conductorBC.ListarUsuariosDisponibles();
            var usuarioActual = _usuarioBC.ListarPorId(idUsuarioActual);

            if (usuarioActual != null && disponibles.All(x => x.IdUsuario != idUsuarioActual))
                disponibles.Insert(0, usuarioActual);

            return disponibles
                .Select(u => new SelectListItem
                {
                    Value = u.IdUsuario.ToString(),
                    Text = $"{u.Username} - {u.Correo}",
                    Selected = u.IdUsuario == idUsuarioActual
                })
                .ToList();
        }
    }
}
