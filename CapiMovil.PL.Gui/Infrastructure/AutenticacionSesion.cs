using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CapiMovil.PL.Gui.Infrastructure
{
    public static class RolesSistema
    {
        public const string Admin = "ADMIN";
        public const string Administrador = "ADMINISTRADOR";
        public const string Conductor = "CONDUCTOR";
        public const string Padre = "PADRE";
        public const string PadreFamilia = "PADRE DE FAMILIA";

        public static readonly string[] Administracion = { Administrador, Admin };
        public static readonly string[] Padres = { Padre, PadreFamilia };
    }

    public static class AutenticacionSesion
    {
        public static string NormalizarRol(string? rol)
        {
            return (rol ?? string.Empty).Trim().ToUpperInvariant();
        }

        public static bool TieneSesionActiva(ISession session)
        {
            string? usuarioId = session.GetString("UsuarioId");
            return !string.IsNullOrWhiteSpace(usuarioId);
        }

        public static bool RolPermitido(ISession session, params string[] rolesPermitidos)
        {
            string rolNormalizado = NormalizarRol(session.GetString("RolNombre"));
            return rolesPermitidos
                .Select(NormalizarRol)
                .Contains(rolNormalizado);
        }

        public static IActionResult? ValidarSesionYRol(Controller controller, params string[] rolesPermitidos)
        {
            if (!TieneSesionActiva(controller.HttpContext.Session))
                return controller.RedirectToAction("Login", "Auth");

            if (!RolPermitido(controller.HttpContext.Session, rolesPermitidos))
                return controller.RedirectToAction("AccesoDenegado", "Auth");

            return null;
        }
    }
}
