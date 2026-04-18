using CapiMovil.BL.BE;
using CapiMovil.DL.DALC;

namespace CapiMovil.BL.BC
{
    public class NotificacionBC
    {
        private readonly NotificacionDALC _notificacionDALC;
        private readonly AuditoriaBC _auditoriaBC;

        public NotificacionBC(NotificacionDALC notificacionDALC, AuditoriaBC auditoriaBC)
        {
            _notificacionDALC = notificacionDALC;
            _auditoriaBC = auditoriaBC;
        }
        public bool RegistrarAutomatica(
            Guid idPadre,
            Guid? idEstudiante,
            string titulo,
            string mensaje,
            string tipoNotificacion,
            string canal = "SISTEMA")
        {
            NotificacionBE entidad = new NotificacionBE
            {
                IdPadre = idPadre,
                IdEstudiante = idEstudiante,
                Titulo = titulo,
                Mensaje = mensaje,
                TipoNotificacion = tipoNotificacion,
                Canal = canal,
                Leido = false
            };

            return Registrar(entidad);
        }
        public List<NotificacionBE> Listar()
        {
            return _notificacionDALC.Listar();
        }

        public NotificacionBE? ListarPorId(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("El id de la notificación es inválido.");

            return _notificacionDALC.ListarPorId(id);
        }

        public bool Registrar(NotificacionBE entidad)
        {
            Validar(entidad, true);
            return _notificacionDALC.Registrar(entidad);
        }

        public bool Actualizar(NotificacionBE entidad)
        {
            if (entidad.IdNotificacion == Guid.Empty)
                throw new ArgumentException("El id de la notificación es inválido.");

            Validar(entidad, false);
            return _notificacionDALC.Actualizar(entidad);
        }

        public bool Eliminar(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("El id de la notificación es inválido.");

            return _notificacionDALC.Eliminar(id);
        }

        public bool MarcarLeida(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("El id de la notificación es inválido.");

            return _notificacionDALC.MarcarLeida(id);
        }

        public List<NotificacionBE> ListarPorPadre(Guid idPadre)
        {
            if (idPadre == Guid.Empty)
                throw new ArgumentException("El id del padre es inválido.");

            return _notificacionDALC.ListarPorPadre(idPadre);
        }

        public List<NotificacionBE> ListarNoLeidasPorPadre(Guid idPadre)
        {
            if (idPadre == Guid.Empty)
                throw new ArgumentException("El id del padre es inválido.");

            return _notificacionDALC.ListarNoLeidasPorPadre(idPadre);
        }

        private void Validar(NotificacionBE entidad, bool esNuevo)
        {
            if (entidad == null)
                throw new ArgumentNullException(nameof(entidad));

            if (!esNuevo && entidad.IdNotificacion == Guid.Empty)
                throw new ArgumentException("El id de la notificación es inválido.");

            if (entidad.IdPadre == Guid.Empty)
                throw new ArgumentException("Debe seleccionar un padre de familia.");

            if (string.IsNullOrWhiteSpace(entidad.Titulo))
                throw new ArgumentException("Debe ingresar el título.");

            entidad.Titulo = entidad.Titulo.Trim();
            if (entidad.Titulo.Length > 150)
                throw new ArgumentException("El título no puede superar los 150 caracteres.");

            if (string.IsNullOrWhiteSpace(entidad.Mensaje))
                throw new ArgumentException("Debe ingresar el mensaje.");

            entidad.Mensaje = entidad.Mensaje.Trim();
            if (entidad.Mensaje.Length > 300)
                throw new ArgumentException("El mensaje no puede superar los 300 caracteres.");

            if (string.IsNullOrWhiteSpace(entidad.TipoNotificacion))
                throw new ArgumentException("Debe seleccionar el tipo de notificación.");

            entidad.TipoNotificacion = entidad.TipoNotificacion.Trim().ToUpper();
            string[] tipos = { "INFO", "ALERTA", "SUBIDA", "BAJADA", "RETRASO", "INCIDENCIA" };
            if (!tipos.Contains(entidad.TipoNotificacion))
                throw new ArgumentException("El tipo de notificación no es válido.");

            if (string.IsNullOrWhiteSpace(entidad.Canal))
                throw new ArgumentException("Debe seleccionar el canal.");

            entidad.Canal = entidad.Canal.Trim().ToUpper();
            string[] canales = { "SISTEMA", "EMAIL", "SMS", "PUSH" };
            if (!canales.Contains(entidad.Canal))
                throw new ArgumentException("El canal de notificación no es válido.");
        }

        private void RegistrarAuditoria(
            string accion,
            Guid? idRegistro,
            object? antes,
            object? despues,
            string observacion)
        {
            _auditoriaBC.RegistrarAutomatica(
                tabla: "Notificacion",
                idRegistro: idRegistro,
                accion: accion,
                datosAntes: antes,
                datosDespues: despues,
                modulo: "Notificaciones",
                observacion: observacion
            );
        }
    }
}