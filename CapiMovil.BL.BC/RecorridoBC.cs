using CapiMovil.BL.BE;
using CapiMovil.DL.DALC;

namespace CapiMovil.BL.BC
{
    public class RecorridoBC : ICrudBC<RecorridoBE>
    {
        private readonly RecorridoDALC _recorridoDALC;
        private readonly AuditoriaBC _auditoriaBC;

        public RecorridoBC(RecorridoDALC recorridoDALC, AuditoriaBC auditoriaBC)
        {
            _recorridoDALC = recorridoDALC;
            _auditoriaBC = auditoriaBC;
        }

        public List<RecorridoBE> Listar()
        {
            return _recorridoDALC.Listar();
        }

        public RecorridoBE? ListarPorId(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id de recorrido inválido.");

            return _recorridoDALC.ListarPorId(id);
        }

        public bool Registrar(RecorridoBE entidad)
        {
            Validar(entidad);

            bool ok = _recorridoDALC.Registrar(entidad);

            if (ok)
            {
                RegistrarAuditoria(
                    "INSERT",
                    entidad.IdRecorrido == Guid.Empty ? null : entidad.IdRecorrido,
                    null,
                    CrearSnapshotRecorrido(entidad),
                    "Se registró el recorrido");
            }

            return ok;
        }

        public bool Actualizar(RecorridoBE entidad)
        {
            if (entidad.IdRecorrido == Guid.Empty)
                throw new ArgumentException("Id de recorrido inválido.");

            Validar(entidad);

            var antes = _recorridoDALC.ListarPorId(entidad.IdRecorrido);
            bool ok = _recorridoDALC.Actualizar(entidad);

            if (ok)
            {
                var despues = _recorridoDALC.ListarPorId(entidad.IdRecorrido);

                RegistrarAuditoria(
                    "UPDATE",
                    entidad.IdRecorrido,
                    CrearSnapshotRecorrido(antes),
                    CrearSnapshotRecorrido(despues),
                    "Se actualizó el recorrido");
            }

            return ok;
        }

        public bool Eliminar(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id de recorrido inválido.");

            var antes = _recorridoDALC.ListarPorId(id);
            bool ok = _recorridoDALC.Eliminar(id);

            if (ok)
            {
                RegistrarAuditoria(
                    "DELETE",
                    id,
                    CrearSnapshotRecorrido(antes),
                    null,
                    "Se eliminó lógicamente el recorrido");
            }

            return ok;
        }

        public bool Iniciar(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id inválido.");

            var antes = _recorridoDALC.ListarPorId(id);
            bool ok = _recorridoDALC.Iniciar(id);

            if (ok)
            {
                var despues = _recorridoDALC.ListarPorId(id);

                RegistrarAuditoria(
                    "UPDATE",
                    id,
                    CrearSnapshotRecorrido(antes),
                    CrearSnapshotRecorrido(despues),
                    "Se inició el recorrido");
            }

            return ok;
        }

        public bool Finalizar(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id inválido.");

            var antes = _recorridoDALC.ListarPorId(id);
            bool ok = _recorridoDALC.Finalizar(id);

            if (ok)
            {
                var despues = _recorridoDALC.ListarPorId(id);

                RegistrarAuditoria(
                    "UPDATE",
                    id,
                    CrearSnapshotRecorrido(antes),
                    CrearSnapshotRecorrido(despues),
                    "Se finalizó el recorrido");
            }

            return ok;
        }

        public bool Cancelar(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id inválido.");

            var antes = _recorridoDALC.ListarPorId(id);
            bool ok = _recorridoDALC.Cancelar(id);

            if (ok)
            {
                var despues = _recorridoDALC.ListarPorId(id);

                RegistrarAuditoria(
                    "UPDATE",
                    id,
                    CrearSnapshotRecorrido(antes),
                    CrearSnapshotRecorrido(despues),
                    "Se canceló el recorrido");
            }

            return ok;
        }

        private static void Validar(RecorridoBE entidad)
        {
            if (entidad == null)
                throw new ArgumentNullException(nameof(entidad));

            if (entidad.IdRuta == Guid.Empty)
                throw new ArgumentException("Debe seleccionar una ruta.");

            if (entidad.IdBus == Guid.Empty)
                throw new ArgumentException("Debe seleccionar un bus.");

            if (entidad.IdConductor == Guid.Empty)
                throw new ArgumentException("Debe seleccionar un conductor.");

            string estado = (entidad.EstadoRecorrido ?? "").Trim().ToUpperInvariant();
            if (estado != "PROGRAMADO" &&
                estado != "EN_CURSO" &&
                estado != "FINALIZADO" &&
                estado != "CANCELADO")
            {
                throw new ArgumentException("El estado del recorrido no es válido.");
            }
        }

        public List<DestinatarioNotificacionBE> ListarDestinatariosPorRecorrido(Guid idRecorrido)
        {
            if (idRecorrido == Guid.Empty)
                throw new ArgumentException("El id del recorrido es inválido.");

            return _recorridoDALC.ListarDestinatariosPorRecorrido(idRecorrido);
        }

        private void RegistrarAuditoria(
            string accion,
            Guid? idRegistro,
            object? antes,
            object? despues,
            string observacion)
        {
            _auditoriaBC.RegistrarAutomatica(
                tabla: "Recorrido",
                idRegistro: idRegistro,
                accion: accion,
                datosAntes: antes,
                datosDespues: despues,
                modulo: "Operaciones",
                observacion: observacion
            );
        }

        private object CrearSnapshotRecorrido(RecorridoBE? r)
        {
            if (r == null) return new { };

            return new
            {
                r.IdRecorrido,
                r.CodigoRecorrido,
                r.IdRuta,
                r.IdBus,
                r.IdConductor,
                r.Fecha,
                r.EstadoRecorrido,
                r.HoraInicioProgramada,
                r.HoraFinProgramada,
                r.HoraInicioReal,
                r.HoraFinReal,
                r.Observaciones,
                r.Estado
            };
        }
    }
}