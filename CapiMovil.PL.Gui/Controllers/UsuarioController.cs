using CapiMovil.BL.BC;
using CapiMovil.BL.BE;
using CapiMovil.PL.Gui.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data.SqlClient;
using System.IO;

namespace CapiMovil.PL.Gui.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly UsuarioBC _usuarioBC;
        private readonly RolBC _rolBC;
        private readonly ConductorBC _conductorBC;
        private readonly PadreFamiliaBC _padreFamiliaBC;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public UsuarioController(
            UsuarioBC usuarioBC,
            RolBC rolBC,
            ConductorBC conductorBC,
            PadreFamiliaBC padreFamiliaBC,
            IWebHostEnvironment webHostEnvironment)
        {
            _usuarioBC = usuarioBC;
            _rolBC = rolBC;
            _conductorBC = conductorBC;
            _padreFamiliaBC = padreFamiliaBC;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public IActionResult Listar()
        {
            var lista = _usuarioBC.Listar();
            return View(lista);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            UsuarioFormViewModel vm = new UsuarioFormViewModel
            {
                Estado = true,
                Roles = ObtenerRoles()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(UsuarioFormViewModel vm)
        {
            if (vm.IdRol == Guid.Empty)
                ModelState.AddModelError(nameof(vm.IdRol), "Debe seleccionar un rol.");

            if (string.IsNullOrWhiteSpace(vm.PasswordNueva))
                ModelState.AddModelError(nameof(vm.PasswordNueva), "La contraseña es obligatoria.");

            if (!ModelState.IsValid)
            {
                vm.Roles = ObtenerRoles();
                return View(vm);
            }

            try
            {
                UsuarioBE usuario = new UsuarioBE
                {
                    IdRol = vm.IdRol,
                    Username = vm.Username,
                    Correo = vm.Correo,
                    PasswordHash = vm.PasswordNueva,
                    Estado = vm.Estado
                };

                bool ok = _usuarioBC.Registrar(usuario);

                if (ok)
                {
                    TempData["ok"] = $"Usuario registrado correctamente.";
                    return RedirectToAction(nameof(Listar));
                }

                ModelState.AddModelError(string.Empty, "No se pudo registrar el usuario.");
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627 || ex.Number == 2601)
                {
                    if (ex.Message.Contains("UQ_Usuario_Correo"))
                        ModelState.AddModelError(nameof(vm.Correo), "El correo ya está registrado.");
                    else if (ex.Message.Contains("UQ_Usuario_Username"))
                        ModelState.AddModelError(nameof(vm.Username), "El nombre de usuario ya está registrado.");
                    else
                        ModelState.AddModelError(string.Empty, "Ya existe un registro con esos datos.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Ocurrió un error al registrar el usuario.");
                }
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Ocurrió un error inesperado.");
            }

            vm.Roles = ObtenerRoles();
            return View(vm);
        }

        [HttpGet]
        public IActionResult Editar(Guid id)
        {
            var usuario = _usuarioBC.ListarPorId(id);

            if (usuario == null)
            {
                TempData["error"] = "Usuario no encontrado.";
                return RedirectToAction(nameof(Listar));
            }

            UsuarioFormViewModel vm = new UsuarioFormViewModel
            {
                IdUsuario = usuario.IdUsuario,
                IdRol = usuario.IdRol,
                CodigoUsuario = usuario.CodigoUsuario,
                Username = usuario.Username,
                Correo = usuario.Correo,
                Estado = usuario.Estado,
                Roles = ObtenerRoles()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(UsuarioFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                var errores = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                ViewBag.SwalError = string.Join(" | ", errores);
                vm.Roles = ObtenerRoles();
                return View(vm);
            }

            try
            {
                UsuarioBE usuario = new UsuarioBE
                {
                    IdUsuario = vm.IdUsuario,
                    IdRol = vm.IdRol,
                    Username = vm.Username,
                    Correo = vm.Correo,
                    Estado = vm.Estado
                };

                bool ok = _usuarioBC.Actualizar(usuario);

                if (ok)
                {
                    TempData["ok"] = "Usuario actualizado correctamente.";
                    return RedirectToAction(nameof(Listar));
                }

                TempData["error"] = "No se pudo actualizar el usuario.";
            }
            catch (Exception ex)
            {

                ViewBag.SwalError = ex.Message;
            }

            ViewBag.SwalError = $"Id={vm.IdUsuario} | Rol={vm.IdRol} | User={vm.Username} | Correo={vm.Correo}";
            vm.Roles = ObtenerRoles();
            return View(vm);
        }


        [HttpGet]
        public IActionResult MiPerfil()
        {
            string? usuarioIdSession = HttpContext.Session.GetString("UsuarioId");

            if (string.IsNullOrEmpty(usuarioIdSession))
                return RedirectToAction("Login", "Auth");

            Guid idUsuario = Guid.Parse(usuarioIdSession);
            UsuarioBE? usuario = _usuarioBC.ListarPorId(idUsuario);

            if (usuario == null)
            {
                TempData["error"] = "Usuario no encontrado.";
                return RedirectToAction("Login", "Auth");
            }

            return View(ConstruirPerfil(usuario));
        }

        [HttpGet]
        public IActionResult EditarPerfil()
        {
            string? usuarioIdSession = HttpContext.Session.GetString("UsuarioId");
            if (!Guid.TryParse(usuarioIdSession, out Guid idUsuario))
                return RedirectToAction("Login", "Auth");

            UsuarioBE? usuario = _usuarioBC.ListarPorId(idUsuario);
            if (usuario == null)
                return RedirectToAction("Login", "Auth");

            return View(MapearEdicion(usuario));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditarPerfil(PerfilEditarViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            try
            {
                UsuarioBE? usuario = _usuarioBC.ListarPorId(vm.IdUsuario);
                if (usuario == null)
                    return RedirectToAction("Login", "Auth");

                usuario.Username = vm.Username.Trim();
                usuario.Correo = vm.Correo.Trim().ToLowerInvariant();
                bool okUsuario = _usuarioBC.Actualizar(usuario);

                bool okEntidad = true;
                string rol = (vm.Rol ?? string.Empty).Trim().ToUpperInvariant();

                if (rol == "CONDUCTOR")
                {
                    ConductorBE? conductor = _conductorBC.ObtenerPorIdUsuario(vm.IdUsuario);
                    if (conductor != null)
                    {
                        conductor.Nombres = vm.Nombres ?? conductor.Nombres;
                        conductor.ApellidoPaterno = vm.ApellidoPaterno ?? conductor.ApellidoPaterno;
                        conductor.ApellidoMaterno = vm.ApellidoMaterno ?? conductor.ApellidoMaterno;
                        conductor.Telefono = vm.Telefono;
                        conductor.Direccion = vm.Direccion;
                        conductor.Licencia = vm.Licencia ?? conductor.Licencia;
                        conductor.CategoriaLicencia = vm.CategoriaLicencia;
                        okEntidad = _conductorBC.Actualizar(conductor);
                    }
                }
                else if (rol is "PADRE" or "PADRE DE FAMILIA")
                {
                    PadreFamiliaBE? padre = _padreFamiliaBC.ObtenerPorIdUsuario(vm.IdUsuario);
                    if (padre != null)
                    {
                        padre.Nombres = vm.Nombres ?? padre.Nombres;
                        padre.ApellidoPaterno = vm.ApellidoPaterno ?? padre.ApellidoPaterno;
                        padre.ApellidoMaterno = vm.ApellidoMaterno ?? padre.ApellidoMaterno;
                        padre.Telefono = vm.Telefono;
                        padre.TelefonoAlterno = vm.TelefonoAlterno;
                        padre.Direccion = vm.Direccion;
                        padre.CorreoContacto = vm.CorreoContacto;
                        okEntidad = _padreFamiliaBC.Actualizar(padre);
                    }
                }

                TempData[okUsuario && okEntidad ? "ok" : "error"] = okUsuario && okEntidad
                    ? "Perfil actualizado correctamente."
                    : "No se pudo actualizar el perfil.";

                HttpContext.Session.SetString("Username", usuario.Username);
                return RedirectToAction(nameof(MiPerfil));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(vm);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ActualizarFotoPerfil(IFormFile? fotoPerfil)
        {
            string? usuarioIdSession = HttpContext.Session.GetString("UsuarioId");
            if (!Guid.TryParse(usuarioIdSession, out Guid idUsuario))
                return RedirectToAction("Login", "Auth");

            if (fotoPerfil == null || fotoPerfil.Length == 0)
            {
                TempData["error"] = "Debe seleccionar una imagen.";
                return RedirectToAction(nameof(MiPerfil));
            }

            string[] extensionesPermitidas = [".jpg", ".jpeg", ".png", ".webp"];
            string extension = Path.GetExtension(fotoPerfil.FileName).ToLowerInvariant();
            if (!extensionesPermitidas.Contains(extension))
            {
                TempData["error"] = "Formato inválido. Use JPG, PNG o WEBP.";
                return RedirectToAction(nameof(MiPerfil));
            }

            if (fotoPerfil.Length > 2 * 1024 * 1024)
            {
                TempData["error"] = "La imagen no debe superar 2MB.";
                return RedirectToAction(nameof(MiPerfil));
            }

            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "perfiles");
            Directory.CreateDirectory(uploadsFolder);
            string nombreArchivo = $"{idUsuario:N}_{DateTime.UtcNow:yyyyMMddHHmmss}{extension}";
            string rutaFisica = Path.Combine(uploadsFolder, nombreArchivo);

            using (FileStream stream = new FileStream(rutaFisica, FileMode.Create))
            {
                fotoPerfil.CopyTo(stream);
            }

            string rutaRelativa = $"/uploads/perfiles/{nombreArchivo}";
            bool ok = _usuarioBC.ActualizarFotoPerfil(idUsuario, rutaRelativa);
            TempData[ok ? "ok" : "error"] = ok ? "Foto de perfil actualizada." : "No se pudo actualizar la foto.";
            if (ok)
                HttpContext.Session.SetString("FotoPerfilUrl", rutaRelativa);

            return RedirectToAction(nameof(MiPerfil));
        }

        [HttpGet]
        public IActionResult CambiarPassword()
        {
            string? usuarioIdSession = HttpContext.Session.GetString("UsuarioId");

            if (string.IsNullOrEmpty(usuarioIdSession))
                return RedirectToAction("Login", "Auth");

            Guid idUsuario = Guid.Parse(usuarioIdSession);

            var usuario = _usuarioBC.ListarPorId(idUsuario);

            if (usuario == null)
            {
                TempData["error"] = "Usuario no encontrado.";
                return RedirectToAction("Login", "Auth");
            }

            UsuarioPasswordViewModel vm = new UsuarioPasswordViewModel
            {
                IdUsuario = usuario.IdUsuario,
                CodigoUsuario = usuario.CodigoUsuario,
                Username = usuario.Username
            };

            return View(vm);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CambiarPassword(UsuarioPasswordViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            try
            {
                string? usuarioIdSession = HttpContext.Session.GetString("UsuarioId");

                if (string.IsNullOrEmpty(usuarioIdSession))
                    return RedirectToAction("Login", "Auth");

                Guid idUsuarioLogueado = Guid.Parse(usuarioIdSession);

                bool ok = _usuarioBC.CambiarPassword(idUsuarioLogueado, vm.PasswordNueva, vm.ConfirmarPassword);

                if (ok)
                {
                    TempData["ok"] = "Contraseña actualizada correctamente.";
                    return RedirigirSegunRol();
                }

                ModelState.AddModelError(string.Empty, "No se pudo actualizar la contraseña.");
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Ocurrió un error al cambiar la contraseña.");
            }

            return View(vm);
        }
        private IActionResult RedirigirSegunRol()
        {
            var rolSesion = (HttpContext.Session.GetString("RolNombre") ?? string.Empty).Trim().ToUpperInvariant();

            if (rolSesion is "ADMIN" or "ADMINISTRADOR")
                return RedirectToAction("Index", "Admin");

            if (rolSesion == "CONDUCTOR")
                return RedirectToAction("Index", "Conductor");

            if (rolSesion is "PADRE" or "PADRE DE FAMILIA")
                return RedirectToAction("Index", "PadreFamilia");

            return RedirectToAction("Index", "Home");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(Guid id)
        {
            try
            {
                bool ok = _usuarioBC.Eliminar(id);

                TempData[ok ? "ok" : "error"] = ok
                    ? "Usuario eliminado correctamente."
                    : "No se pudo eliminar el usuario.";
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }

            return RedirectToAction(nameof(Listar));
        }

        private List<SelectListItem> ObtenerRoles()
        {
            return _rolBC.Listar()
                .Select(r => new SelectListItem
                {
                    Value = r.IdRol.ToString(),
                    Text = r.Nombre
                })
                .ToList();
        }

        private PerfilViewModel ConstruirPerfil(UsuarioBE usuario)
        {
            string rol = usuario.Rol?.Nombre ?? "USUARIO";
            string rolNormalizado = rol.Trim().ToUpperInvariant();

            PerfilViewModel vm = new()
            {
                IdUsuario = usuario.IdUsuario,
                Rol = rol,
                CodigoPerfil = usuario.CodigoUsuario,
                Username = usuario.Username,
                Correo = usuario.Correo,
                Estado = usuario.Estado,
                FotoPerfilUrl = usuario.FotoPerfilUrl,
                NombreCompleto = usuario.Username
            };

            if (rolNormalizado == "CONDUCTOR")
            {
                ConductorBE? conductor = _conductorBC.ObtenerPorIdUsuario(usuario.IdUsuario);
                if (conductor != null)
                {
                    vm.IdEntidadRol = conductor.IdConductor;
                    vm.CodigoPerfil = conductor.CodigoConductor;
                    vm.NombreCompleto = conductor.NombreCompleto;
                    vm.Telefono = conductor.Telefono;
                    vm.Direccion = conductor.Direccion;
                    vm.Documento = conductor.DNI;
                    vm.Licencia = conductor.Licencia;
                    vm.CategoriaLicencia = conductor.CategoriaLicencia;
                    vm.Estado = conductor.Estado;
                }
            }
            else if (rolNormalizado is "PADRE" or "PADRE DE FAMILIA")
            {
                PadreFamiliaBE? padre = _padreFamiliaBC.ObtenerPorIdUsuario(usuario.IdUsuario);
                if (padre != null)
                {
                    vm.IdEntidadRol = padre.IdPadre;
                    vm.CodigoPerfil = padre.CodigoPadre;
                    vm.NombreCompleto = padre.NombreCompleto;
                    vm.Telefono = padre.Telefono;
                    vm.TelefonoAlterno = padre.TelefonoAlterno;
                    vm.Direccion = padre.Direccion;
                    vm.CorreoContacto = padre.CorreoContacto;
                    vm.Documento = padre.DNI;
                    vm.Estado = padre.Estado;
                }
            }

            vm.InicialesAvatar = ObtenerIniciales(vm.NombreCompleto);
            return vm;
        }

        private PerfilEditarViewModel MapearEdicion(UsuarioBE usuario)
        {
            PerfilViewModel perfil = ConstruirPerfil(usuario);

            return new PerfilEditarViewModel
            {
                IdUsuario = perfil.IdUsuario,
                IdEntidadRol = perfil.IdEntidadRol,
                Rol = perfil.Rol,
                Username = perfil.Username,
                Correo = perfil.Correo,
                Estado = perfil.Estado,
                FotoPerfilUrl = perfil.FotoPerfilUrl,
                InicialesAvatar = perfil.InicialesAvatar,
                Telefono = perfil.Telefono,
                TelefonoAlterno = perfil.TelefonoAlterno,
                Direccion = perfil.Direccion,
                CorreoContacto = perfil.CorreoContacto,
                Licencia = perfil.Licencia,
                CategoriaLicencia = perfil.CategoriaLicencia,
                Nombres = perfil.NombreCompleto.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? perfil.NombreCompleto,
                ApellidoPaterno = perfil.NombreCompleto.Split(' ', StringSplitOptions.RemoveEmptyEntries).Skip(1).FirstOrDefault(),
                ApellidoMaterno = perfil.NombreCompleto.Split(' ', StringSplitOptions.RemoveEmptyEntries).Skip(2).FirstOrDefault()
            };
        }

        private static string ObtenerIniciales(string texto)
        {
            string[] partes = (texto ?? string.Empty)
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (partes.Length == 0) return "CM";
            if (partes.Length == 1) return partes[0][..1].ToUpperInvariant();
            return (partes[0][..1] + partes[1][..1]).ToUpperInvariant();
        }


    }
}
