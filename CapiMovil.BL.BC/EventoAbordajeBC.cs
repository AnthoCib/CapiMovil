using CapiMovil.BL.BE;
using CapiMovil.DL.DALC;

namespace CapiMovil.BL.BC
{
    public class EventoAbordajeBC : ICrudBC<EventoAbordajeBE>
    {
        private readonly EventoAbordajeDALC _eventoAbordajeDALC;
        private readonly NotificacionBC _notificacionBC;
        private readonly EstudianteBC _estudianteBC;
        private readonly AuditoriaBC _auditoriaBC;

        public EventoAbordajeBC(
            EventoAbordajeDALC eventoAbordajeDALC,
            NotificacionBC notificacionBC,
            EstudianteBC estudianteBC,
            AuditoriaBC auditoriaBC)
        {
            _eventoAbordajeDALC = eventoAbordajeDALC;
            _notificacionBC = notificacionBC;
            _estudianteBC = estudianteBC;
            _auditoriaBC = auditoriaBC;
        }

        public List<EventoAbordajeBE> Listar()
        {
            return _eventoAbordajeDALC.Listar();
        }

        public EventoAbordajeBE? ListarPorId(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id de evento inválido.");

            return _eventoAbordajeDALC.ListarPorId(id);
        }

        public bool Registrar(EventoAbordajeBE entidad)
        {
            Validar(entidad);

            bool ok = _eventoAbordajeDALC.Registrar(entidad);

            if (ok)
            {
                RegistrarAuditoria(
                    "INSERT",
                    entidad.IdEvento == Guid.Empty ? null : entidad.IdEvento,
                    null,
                    CrearSnapshotEvento(entidad),
                    "Se registró un evento de abordaje"
                );

                Guid idPadre = _estudianteBC.ObtenerPadrePorEstudiante(entidad.IdEstudiante);

                if (idPadre != Guid.Empty)
                {
                    string tipo = (entidad.TipoEvento ?? "").Trim().ToUpperInvariant();
                    string titulo = string.Empty;
                    string mensaje = string.Empty;

                    if (tipo == "SUBIDA")
                    {
                        titulo = "Estudiante abordó la unidad";
                        mensaje = "Se registró la subida del estudiante a la unidad de transporte.";
                    }
                    else if (tipo == "BAJADA")
                    {
                        titulo = "Estudiante descendió de la unidad";
                        mensaje = "Se registró la bajada del estudiante de la unidad de transporte.";
                    }
                    else if (tipo == "AUSENTE")
                    {
                        titulo = "Estudiante ausente";
                        mensaje = "Se registró al estudiante como ausente en el recorrido.";
                    }
                    else if (tipo == "NO_ABORDO")
                    {
                        titulo = "Estudiante no abordó";
                        mensaje = "Se registró que el estudiante no abordó la unidad.";
                    }

                    if (!string.IsNullOrWhiteSpace(titulo))
                    {
                        _notificacionBC.RegistrarAutomatica(
                            idPadre,
                            entidad.IdEstudiante,
                            titulo,
                            mensaje,
                            tipo == "NO_ABORDO" ? "ALERTA" : tipo,
                            "SISTEMA"
                        );
                    }
                }
            }

            return ok;
        }

        public bool Actualizar(EventoAbordajeBE entidad)
        {
            if (entidad.IdEvento == Guid.Empty)
                throw new ArgumentException("Id de evento inválido.");

            Validar(entidad);

            var antes = _eventoAbordajeDALC.ListarPorId(entidad.IdEvento);
            bool ok = _eventoAbordajeDALC.Actualizar(entidad);

            if (ok)
            {
                var despues = _eventoAbordajeDALC.ListarPorId(entidad.IdEvento);

                RegistrarAuditoria(
                    "UPDATE",
                    entidad.IdEvento,
                    CrearSnapshotEvento(antes),
                    CrearSnapshotEvento(despues),
                    "Se actualizó un evento de abordaje"
                );
            }

            return ok;
        }

        public bool Eliminar(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id de evento inválido.");

            var antes = _eventoAbordajeDALC.ListarPorId(id);
            bool ok = _eventoAbordajeDALC.Eliminar(id);

            if (ok)
            {
                RegistrarAuditoria(
                    "DELETE",
                    id,
                    CrearSnapshotEvento(antes),
                    null,
                    "Se eliminó lógicamente un evento de abordaje"
                );
            }

            return ok;
        }

        private static void Validar(EventoAbordajeBE entidad)
        {
            if (entidad == null)
                throw new ArgumentNullException(nameof(entidad));

            if (entidad.IdRecorrido == Guid.Empty)
                throw new ArgumentException("Debe seleccionar un recorrido.");

            if (entidad.IdEstudiante == Guid.Empty)
                throw new ArgumentException("Debe seleccionar un estudiante.");

            string tipo = (entidad.TipoEvento ?? "").Trim().ToUpperInvariant();
            if (tipo != "SUBIDA" &&
                tipo != "BAJADA" &&
                tipo != "AUSENTE" &&
                tipo != "NO_ABORDO")
            {
                throw new ArgumentException("El tipo de evento no es válido.");
            }
        }

        private void RegistrarAuditoria(
            string accion,
            Guid? idRegistro,
            object? antes,
            object? despues,
            string observacion)
        {
            _auditoriaBC.RegistrarAutomatica(
                tabla: "EventoAbordaje",
                idRegistro: idRegistro,
                accion: accion,
                datosAntes: antes,
                datosDespues: despues,
                modulo: "Operaciones",
                observacion: observacion
            );
        }

        private object CrearSnapshotEvento(EventoAbordajeBE? e)
        {
            if (e == null) return new { };

            return new
            {
                e.IdEvento,
                e.IdRecorrido,
                e.IdEstudiante,
                e.TipoEvento,
                e.FechaHora,
                e.Observacion,
                e.Estado
            };
        }
    }
}