using CapiMovil.BL.BC;
using CapiMovil.BL.BE;
using CapiMovil.PL.Gui.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CapiMovil.PL.Gui.Controllers
{
    public class PadreFamiliaController : Controller
    {
        private readonly PadreFamiliaBC _padreFamiliaBC;
        private readonly UsuarioBC _usuarioBC;
        private readonly EstudianteBC _estudianteBC;
        private readonly NotificacionBC _notificacionBC;
        private readonly EventoAbordajeBC _eventoAbordajeBC;

        public PadreFamiliaController(
            PadreFamiliaBC padreFamiliaBC,
            UsuarioBC usuarioBC,
            EstudianteBC estudianteBC,
            NotificacionBC notificacionBC,
            EventoAbordajeBC eventoAbordajeBC)
        {
            _padreFamiliaBC = padreFamiliaBC;
            _usuarioBC = usuarioBC;
            _estudianteBC = estudianteBC;
            _notificacionBC = notificacionBC;
            _eventoAbordajeBC = eventoAbordajeBC;
        }

        public IActionResult Index()
        {
            IActionResult? acceso = ValidarSesionYRol("PADRE", "PADRE DE FAMILIA");
            if (acceso != null)
                return acceso;

            PadreFamiliaBE? padre = ObtenerPadreAutenticado();

            ViewBag.CantidadHijos = 0;
            ViewBag.CantidadNotificaciones = 0;
            ViewBag.PadreNombre = "Padre de Familia";

            if (padre != null)
            {
                ViewBag.PadreNombre = padre.NombreCompleto;
                ViewBag.CantidadHijos = _estudianteBC.Listar().Count(e => e.IdPadre == padre.IdPadre);
                ViewBag.CantidadNotificaciones = _notificacionBC.Listar().Count(n => n.IdPadre == padre.IdPadre && !n.Leido);
            }

            return View();
        }

        [HttpGet]
        public IActionResult MisHijos()
        {
            IActionResult? acceso = ValidarSesionYRol("PADRE", "PADRE DE FAMILIA");
            if (acceso != null)
                return acceso;

            PadreFamiliaBE? padre = ObtenerPadreAutenticado();
            List<EstudianteBE> hijos = padre == null
                ? new List<EstudianteBE>()
                : _estudianteBC.Listar().Where(e => e.IdPadre == padre.IdPadre).ToList();

            return View(hijos);
        }

        [HttpGet]
        public IActionResult Notificaciones()
        {
            IActionResult? acceso = ValidarSesionYRol("PADRE", "PADRE DE FAMILIA");
            if (acceso != null)
                return acceso;

            PadreFamiliaBE? padre = ObtenerPadreAutenticado();
            List<NotificacionBE> notificaciones = padre == null
                ? new List<NotificacionBE>()
                : _notificacionBC.Listar().Where(n => n.IdPadre == padre.IdPadre).ToList();

            return View(notificaciones);
        }

        [HttpGet]
        public IActionResult MiRuta()
        {
            IActionResult? acceso = ValidarSesionYRol("PADRE", "PADRE DE FAMILIA");
            if (acceso != null)
                return acceso;

            return View();
        }

        [HttpGet]
        public IActionResult MiParadero()
        {
            IActionResult? acceso = ValidarSesionYRol("PADRE", "PADRE DE FAMILIA");
            if (acceso != null)
                return acceso;

            return View();
        }

        [HttpGet]
        public IActionResult MisEventos()
        {
            IActionResult? acceso = ValidarSesionYRol("PADRE", "PADRE DE FAMILIA");
            if (acceso != null)
                return acceso;

            PadreFamiliaBE? padre = ObtenerPadreAutenticado();
            if (padre == null)
                return View(new List<EventoAbordajeBE>());

            HashSet<Guid> idsHijos = _estudianteBC.Listar()
                .Where(e => e.IdPadre == padre.IdPadre)
                .Select(e => e.IdEstudiante)
                .ToHashSet();

            List<EventoAbordajeBE> eventos = _eventoAbordajeBC.Listar()
                .Where(e => idsHijos.Contains(e.IdEstudiante))
                .OrderByDescending(e => e.FechaHora)
                .ToList();

            return View(eventos);
        }

        [HttpGet]
        public IActionResult Listar()
        {
            IActionResult? acceso = ValidarSesionYRol("ADMINISTRADOR");
            if (acceso != null) return acceso;

            var lista = _padreFamiliaBC.Listar();
            return View(lista);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            IActionResult? acceso = ValidarSesionYRol("ADMINISTRADOR");
            if (acceso != null) return acceso;

            PadreFamiliaFormViewModel vm = new()
            {
                Estado = true,
                Usuarios = ObtenerUsuariosDisponibles()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(PadreFamiliaFormViewModel vm)
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
                PadreFamiliaBE entidad = new()
                {
                    IdUsuario = vm.IdUsuario,
                    Nombres = vm.Nombres,
                    ApellidoPaterno = vm.ApellidoPaterno,
                    ApellidoMaterno = vm.ApellidoMaterno,
                    DNI = vm.DNI,
                    Telefono = vm.Telefono,
                    TelefonoAlterno = vm.TelefonoAlterno,
                    Direccion = vm.Direccion,
                    CorreoContacto = vm.CorreoContacto,
                    Estado = vm.Estado
                };

                bool ok = _padreFamiliaBC.Registrar(entidad);

                TempData[ok ? "ok" : "error"] = ok
                    ? "Padre de familia registrado correctamente."
                    : "No se pudo registrar el padre de familia.";

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

            var entidad = _padreFamiliaBC.ListarPorId(id);

            if (entidad == null)
            {
                TempData["error"] = "Padre de familia no encontrado.";
                return RedirectToAction(nameof(Listar));
            }

            PadreFamiliaFormViewModel vm = new()
            {
                IdPadre = entidad.IdPadre,
                IdUsuario = entidad.IdUsuario,
                CodigoPadre = entidad.CodigoPadre,
                Nombres = entidad.Nombres,
                ApellidoPaterno = entidad.ApellidoPaterno,
                ApellidoMaterno = entidad.ApellidoMaterno,
                DNI = entidad.DNI,
                Telefono = entidad.Telefono,
                TelefonoAlterno = entidad.TelefonoAlterno,
                Direccion = entidad.Direccion,
                CorreoContacto = entidad.CorreoContacto,
                Estado = entidad.Estado,
                Usuarios = ObtenerUsuariosParaEdicion(entidad.IdUsuario)
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(PadreFamiliaFormViewModel vm)
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
                PadreFamiliaBE entidad = new()
                {
                    IdPadre = vm.IdPadre,
                    IdUsuario = vm.IdUsuario,
                    CodigoPadre = vm.CodigoPadre,
                    Nombres = vm.Nombres,
                    ApellidoPaterno = vm.ApellidoPaterno,
                    ApellidoMaterno = vm.ApellidoMaterno,
                    DNI = vm.DNI,
                    Telefono = vm.Telefono,
                    TelefonoAlterno = vm.TelefonoAlterno,
                    Direccion = vm.Direccion,
                    CorreoContacto = vm.CorreoContacto,
                    Estado = vm.Estado
                };

                bool ok = _padreFamiliaBC.Actualizar(entidad);

                TempData[ok ? "ok" : "error"] = ok
                    ? "Padre de familia actualizado correctamente."
                    : "No se pudo actualizar el padre de familia.";

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
                bool ok = _padreFamiliaBC.Eliminar(id);

                TempData[ok ? "ok" : "error"] = ok
                    ? "Padre de familia eliminado correctamente."
                    : "No se pudo eliminar el padre de familia.";
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

        private PadreFamiliaBE? ObtenerPadreAutenticado()
        {
            string? usuarioId = HttpContext.Session.GetString("UsuarioId");
            if (!Guid.TryParse(usuarioId, out Guid idUsuario))
                return null;

            return _padreFamiliaBC.Listar().FirstOrDefault(p => p.IdUsuario == idUsuario);
        }

        private List<SelectListItem> ObtenerUsuariosDisponibles()
        {
            return _padreFamiliaBC.ListarUsuariosDisponibles()
                .Select(u => new SelectListItem
                {
                    Value = u.IdUsuario.ToString(),
                    Text = $"{u.Username} - {u.Correo}"
                })
                .ToList();
        }

        private List<SelectListItem> ObtenerUsuariosParaEdicion(Guid idUsuarioActual)
        {
            var disponibles = _padreFamiliaBC.ListarUsuariosDisponibles();
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
