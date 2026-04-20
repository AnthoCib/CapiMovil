using CapiMovil.BL.BE;
using CapiMovil.DL.DALC;
using Microsoft.AspNetCore.Http;
using System.Text.Json;


namespace CapiMovil.BL.BC
{
    public class AuditoriaBC
    {
        private readonly AuditoriaDALC _auditoriaDALC;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditoriaBC(AuditoriaDALC auditoriaDALC, IHttpContextAccessor httpContextAccessor)
        {
            _auditoriaDALC = auditoriaDALC;
            _httpContextAccessor = httpContextAccessor;
        }

        public List<AuditoriaBE> Listar()
        {
            return _auditoriaDALC.Listar();
        }

        public AuditoriaBE? ListarPorId(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("El id de auditoría es inválido.");

            return _auditoriaDALC.ListarPorId(id);
        }

        public bool Registrar(AuditoriaBE entidad)
        {
            Validar(entidad);
            return _auditoriaDALC.Registrar(entidad);
        }

        public List<AuditoriaBE> ListarPorTabla(string tabla)
        {
            if (string.IsNullOrWhiteSpace(tabla))
                throw new ArgumentException("La tabla es obligatoria.");

            return _auditoriaDALC.ListarPorTabla(tabla.Trim());
        }

        public List<AuditoriaBE> ListarPorAccion(string accion)
        {
            if (string.IsNullOrWhiteSpace(accion))
                throw new ArgumentException("La acción es obligatoria.");

            return _auditoriaDALC.ListarPorAccion(accion.Trim().ToUpper());
        }

        public List<AuditoriaBE> ListarPorUsuario(Guid usuarioId)
        {
            if (usuarioId == Guid.Empty)
                throw new ArgumentException("El usuario es inválido.");

            return _auditoriaDALC.ListarPorUsuario(usuarioId);
        }

        public bool RegistrarAutomatica(
            string tabla,
            Guid? idRegistro,
            string accion,
            object? datosAntes = null,
            object? datosDespues = null,
            Guid? usuarioId = null,
            string? nombreUsuario = null,
            string? ip = null,
            string? userAgent = null,
            string? modulo = null,
            string? observacion = null)
        {
            var http = _httpContextAccessor.HttpContext;

            Guid? usuarioIdFinal = usuarioId;
            string? nombreUsuarioFinal = nombreUsuario;
            string? ipFinal = ip;
            string? userAgentFinal = userAgent;

            if (http != null)
            {
                if (!usuarioIdFinal.HasValue)
                {
                    string? sessionUsuarioId = ObtenerSessionString(http.Session, "UsuarioId");
                    if (Guid.TryParse(sessionUsuarioId, out Guid guidUsuario))
                        usuarioIdFinal = guidUsuario;
                }

                if (string.IsNullOrWhiteSpace(nombreUsuarioFinal))
                    nombreUsuarioFinal = ObtenerSessionString(http.Session, "Username");

                if (string.IsNullOrWhiteSpace(ipFinal))
                    ipFinal = http.Connection.RemoteIpAddress?.ToString();

                if (string.IsNullOrWhiteSpace(userAgentFinal))
                    userAgentFinal = http.Request.Headers["User-Agent"].ToString();
            }

            AuditoriaBE entidad = new AuditoriaBE
            {
                Tabla = tabla,
                IdRegistro = idRegistro,
                Accion = accion,
                DatosAntes = datosAntes == null ? null : JsonSerializer.Serialize(datosAntes),
                DatosDespues = datosDespues == null ? null : JsonSerializer.Serialize(datosDespues),
                UsuarioId = usuarioIdFinal,
                NombreUsuario = nombreUsuarioFinal,
                Ip = ipFinal,
                UserAgent = userAgentFinal,
                Modulo = modulo,
                Observacion = observacion
            };

            return Registrar(entidad);
        }

        private void Validar(AuditoriaBE entidad)
        {
            if (entidad == null)
                throw new ArgumentNullException(nameof(entidad));

            if (string.IsNullOrWhiteSpace(entidad.Tabla))
                throw new ArgumentException("La tabla es obligatoria.");

            entidad.Tabla = entidad.Tabla.Trim();
            if (entidad.Tabla.Length > 100)
                throw new ArgumentException("La tabla no puede superar los 100 caracteres.");

            if (string.IsNullOrWhiteSpace(entidad.Accion))
                throw new ArgumentException("La acción es obligatoria.");

            entidad.Accion = entidad.Accion.Trim().ToUpper();
            string[] acciones = { "INSERT", "UPDATE", "DELETE", "LOGIN", "LOGOUT", "RESTORE", "OTRO" };
            if (!acciones.Contains(entidad.Accion))
                throw new ArgumentException("La acción no es válida.");

            if (!string.IsNullOrWhiteSpace(entidad.NombreUsuario) && entidad.NombreUsuario.Length > 120)
                throw new ArgumentException("El nombre de usuario no puede superar los 120 caracteres.");

            if (!string.IsNullOrWhiteSpace(entidad.Ip) && entidad.Ip.Length > 50)
                throw new ArgumentException("La IP no puede superar los 50 caracteres.");

            if (!string.IsNullOrWhiteSpace(entidad.UserAgent) && entidad.UserAgent.Length > 300)
                throw new ArgumentException("El UserAgent no puede superar los 300 caracteres.");

            if (!string.IsNullOrWhiteSpace(entidad.Modulo) && entidad.Modulo.Length > 100)
                throw new ArgumentException("El módulo no puede superar los 100 caracteres.");

            if (!string.IsNullOrWhiteSpace(entidad.Observacion) && entidad.Observacion.Length > 250)
                throw new ArgumentException("La observación no puede superar los 250 caracteres.");
        }

        private static string? ObtenerSessionString(ISession session, string key)
        {
            if (session == null || string.IsNullOrWhiteSpace(key))
                return null;

            if (session.TryGetValue(key, out byte[]? value) && value != null)
                return System.Text.Encoding.UTF8.GetString(value);

            return null;
        }
    }
}