using CapiMovil.BL.BC;
using CapiMovil.BL.BE;
using CapiMovil.PL.Gui.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data.SqlClient;

namespace CapiMovil.PL.Gui.Controllers
{
    public class PadreFamiliaController : Controller
    {
        private readonly PadreFamiliaBC _padreFamiliaBC;
        private readonly UsuarioBC _usuarioBC;

        public PadreFamiliaController(PadreFamiliaBC padreFamiliaBC, UsuarioBC usuarioBC)
        {
            _padreFamiliaBC = padreFamiliaBC;
            _usuarioBC = usuarioBC;
        }


        public IActionResult Index()
        {
            string? usuarioId = HttpContext.Session.GetString("UsuarioId");
            string? rol = HttpContext.Session.GetString("RolNombre");

            if (string.IsNullOrEmpty(usuarioId))
                return RedirectToAction("Login", "Auth");

            string rolNormalizado = (rol ?? "").Trim().ToUpperInvariant();

            if (rolNormalizado != "PADRE" && rolNormalizado != "PADRE DE FAMILIA")
                return RedirectToAction("AccesoDenegado", "Auth");

            return View();
        }

        [HttpGet]
        public IActionResult Listar()
        {
            var lista = _padreFamiliaBC.Listar();
            return View(lista);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            PadreFamiliaFormViewModel vm = new PadreFamiliaFormViewModel
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
            if (!ModelState.IsValid)
            {
                vm.Usuarios = ObtenerUsuariosDisponibles();
                return View(vm);
            }
            if (vm.IdUsuario == Guid.Empty)
                ModelState.AddModelError(nameof(vm.IdUsuario), "Debe seleccionar un usuario.");

            try
            {
                PadreFamiliaBE entidad = new PadreFamiliaBE
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
            catch (SqlException ex)
            {
                if (ex.Number == 2627 || ex.Number == 2601)
                {
                    if (ex.Message.Contains("UQ_Padre_DNI"))
                    {
                        ModelState.AddModelError(nameof(vm.DNI), "El DNI ya está registrado.");
                        ViewBag.SwalError = "El DNI ya está registrado.";
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Ya existe un registro con esos datos.");
                        ViewBag.SwalError = "Ya existe un registro con esos datos.";
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Ocurrió un error al registrar al padre de familia.");
                    ViewBag.SwalError = "Ocurrió un error al registrar al padre de familia.";
                }

            }


            vm.Usuarios = ObtenerUsuariosDisponibles();
            return View(vm);
        }

        [HttpGet]
        public IActionResult Editar(Guid id)
        {
            var entidad = _padreFamiliaBC.ListarPorId(id);

            if (entidad == null)
            {
                TempData["error"] = "Padre de familia no encontrado.";
                return RedirectToAction(nameof(Listar));
            }

            PadreFamiliaFormViewModel vm = new PadreFamiliaFormViewModel
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
                Usuarios = ObtenerUsuarios()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(PadreFamiliaFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Usuarios = ObtenerUsuarios();
                return View(vm);
            }

            try
            {
                PadreFamiliaBE entidad = new PadreFamiliaBE
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

        // Este método obtiene solo los usuarios que no están vinculados a ningún padre de familia,
        // para evitar conflictos al editar.
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
    }
}