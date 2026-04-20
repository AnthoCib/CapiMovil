using CapiMovil.BL.BE;
using CapiMovil.DL.DALC;

namespace CapiMovil.BL.BC
{
    public class IncidenciaBC
    {
        private readonly IncidenciaDALC _incidenciaDALC;

        private readonly NotificacionBC _notificacionBC;
        private readonly RecorridoBC _recorridoBC;
        private readonly AuditoriaBC _auditoriaBC;
        
        public IncidenciaBC(
            IncidenciaDALC incidenciaDALC,
            NotificacionBC notificacionBC,
            RecorridoBC recorridoBC,
            AuditoriaBC auditoriaBC)
        {
            _incidenciaDALC = incidenciaDALC;
            _notificacionBC = notificacionBC;
            _recorridoBC = recorridoBC;
            _auditoriaBC = auditoriaBC;
        }

        public List<IncidenciaBE> Listar()
        {
            return _incidenciaDALC.Listar();
        }

        public IncidenciaBE? ListarPorId(Guid idIncidencia)
        {
            if (idIncidencia == Guid.Empty)
                throw new ArgumentException("El id de la incidencia es inválido.");

            return _incidenciaDALC.ListarPorId(idIncidencia);
        }

        public bool Registrar(IncidenciaBE entidad)
        {
            ValidarEntidad(entidad, esNuevo: true);

            if (string.IsNullOrWhiteSpace(entidad.EstadoIncidencia))
                entidad.EstadoIncidencia = "PENDIENTE";

            if (string.IsNullOrWhiteSpace(entidad.Prioridad))
                entidad.Prioridad = "MEDIA";

            bool ok = _incidenciaDALC.Registrar(entidad);

            if (ok)
            {
                RegistrarAuditoria(
                    "INSERT",
                    entidad.IdIncidencia == Guid.Empty ? null : entidad.IdIncidencia,
                    null,
                    CrearSnapshotIncidencia(entidad),
                    "Se registró una incidencia"
                );

                var destinatarios = _recorridoBC.ListarDestinatariosPorRecorrido(entidad.IdRecorrido);

                foreach (var item in destinatarios)
                {
                    _notificacionBC.RegistrarAutomatica(
                        item.IdPadre,
                        item.IdEstudiante,
                        "Nueva incidencia en el recorrido",
                        $"Se registró una incidencia de tipo {entidad.TipoIncidencia}: {entidad.Descripcion}",
                        "INCIDENCIA",
                        "SISTEMA"
                    );
                }
            }

            return ok;
        }

        public bool Actualizar(IncidenciaBE entidad)
        {
            if (entidad.IdIncidencia == Guid.Empty)
                throw new ArgumentException("El id de la incidencia es inválido.");

            ValidarEntidad(entidad, esNuevo: false);

            var antes = _incidenciaDALC.ListarPorId(entidad.IdIncidencia);
            bool ok = _incidenciaDALC.Actualizar(entidad);

            if (ok)
            {
                var despues = _incidenciaDALC.ListarPorId(entidad.IdIncidencia);

                RegistrarAuditoria(
                    "UPDATE",
                    entidad.IdIncidencia,
                    CrearSnapshotIncidencia(antes),
                    CrearSnapshotIncidencia(despues),
                    "Se actualizó una incidencia"
                );
            }

            return ok;
        }
        public bool CambiarEstado(Guid idIncidencia, string estadoIncidencia)
        {
            if (idIncidencia == Guid.Empty)
                throw new ArgumentException("El id de la incidencia es inválido.");

            ValidarEstado(estadoIncidencia);

            var antes = _incidenciaDALC.ListarPorId(idIncidencia);
            bool ok = _incidenciaDALC.CambiarEstado(idIncidencia, estadoIncidencia);

            if (ok)
            {
                var despues = _incidenciaDALC.ListarPorId(idIncidencia);

                RegistrarAuditoria(
                    "UPDATE",
                    idIncidencia,
                    CrearSnapshotIncidencia(antes),
                    CrearSnapshotIncidencia(despues),
                    $"Se cambió el estado de la incidencia a {estadoIncidencia?.Trim().ToUpper()}"
                );
            }

            return ok;
        }

        public bool Cerrar(Guid idIncidencia, string solucion)
        {
            if (idIncidencia == Guid.Empty)
                throw new ArgumentException("El id de la incidencia es inválido.");

            if (string.IsNullOrWhiteSpace(solucion))
                throw new ArgumentException("Debe ingresar la solución de la incidencia.");

            if (solucion.Length > 300)
                throw new ArgumentException("La solución no puede superar los 300 caracteres.");

            var antes = _incidenciaDALC.ListarPorId(idIncidencia);
            bool ok = _incidenciaDALC.Cerrar(idIncidencia, solucion.Trim());

            if (ok)
            {
                var despues = _incidenciaDALC.ListarPorId(idIncidencia);

                RegistrarAuditoria(
                    "UPDATE",
                    idIncidencia,
                    CrearSnapshotIncidencia(antes),
                    CrearSnapshotIncidencia(despues),
                    "Se cerró una incidencia"
                );
            }

            return ok;
        }

        public bool Eliminar(Guid idIncidencia)
        {
            if (idIncidencia == Guid.Empty)
                throw new ArgumentException("El id de la incidencia es inválido.");

            var antes = _incidenciaDALC.ListarPorId(idIncidencia);
            bool ok = _incidenciaDALC.Eliminar(idIncidencia);

            if (ok)
            {
                RegistrarAuditoria(
                    "DELETE",
                    idIncidencia,
                    CrearSnapshotIncidencia(antes),
                    null,
                    "Se eliminó lógicamente una incidencia"
                );
            }

            return ok;
        }

        private void ValidarEntidad(IncidenciaBE entidad, bool esNuevo)
        {
            if (entidad == null)
                throw new ArgumentNullException(nameof(entidad));

            if (!esNuevo && entidad.IdIncidencia == Guid.Empty)
                throw new ArgumentException("El id de la incidencia es inválido.");

            if (entidad.IdRecorrido == Guid.Empty)
                throw new ArgumentException("Debe seleccionar un recorrido.");

            if (entidad.IdConductor == Guid.Empty)
                throw new ArgumentException("Debe seleccionar un conductor.");

            if (string.IsNullOrWhiteSpace(entidad.TipoIncidencia))
                throw new ArgumentException("Debe seleccionar el tipo de incidencia.");

            if (entidad.TipoIncidencia.Length > 50)
                throw new ArgumentException("El tipo de incidencia no puede superar los 50 caracteres.");

            if (string.IsNullOrWhiteSpace(entidad.Descripcion))
                throw new ArgumentException("Debe ingresar la descripción de la incidencia.");

            entidad.Descripcion = entidad.Descripcion.Trim();

            if (entidad.Descripcion.Length > 300)
                throw new ArgumentException("La descripción no puede superar los 300 caracteres.");

            if (entidad.FechaHora == DateTime.MinValue)
                throw new ArgumentException("Debe ingresar la fecha y hora de la incidencia.");

            if (string.IsNullOrWhiteSpace(entidad.EstadoIncidencia))
                throw new ArgumentException("Debe seleccionar el estado de la incidencia.");

            entidad.EstadoIncidencia = entidad.EstadoIncidencia.Trim().ToUpper();
            ValidarEstado(entidad.EstadoIncidencia);

            if (string.IsNullOrWhiteSpace(entidad.Prioridad))
                throw new ArgumentException("Debe seleccionar la prioridad.");

            entidad.Prioridad = entidad.Prioridad.Trim().ToUpper();
            ValidarPrioridad(entidad.Prioridad);

            if (!string.IsNullOrWhiteSpace(entidad.Solucion))
            {
                entidad.Solucion = entidad.Solucion.Trim();

                if (entidad.Solucion.Length > 300)
                    throw new ArgumentException("La solución no puede superar los 300 caracteres.");
            }

            if (entidad.EstadoIncidencia == "CERRADA" && string.IsNullOrWhiteSpace(entidad.Solucion))
                throw new ArgumentException("Debe ingresar la solución para cerrar la incidencia.");
        }

        private void ValidarEstado(string estadoIncidencia)
        {
            string[] estadosValidos = { "PENDIENTE", "ATENDIDA", "CERRADA" };

            if (!estadosValidos.Contains(estadoIncidencia?.Trim().ToUpper()))
                throw new ArgumentException("El estado de la incidencia no es válido.");
        }

        private void ValidarPrioridad(string prioridad)
        {
            string[] prioridadesValidas = { "BAJA", "MEDIA", "ALTA", "CRITICA" };

            if (!prioridadesValidas.Contains(prioridad?.Trim().ToUpper()))
                throw new ArgumentException("La prioridad de la incidencia no es válida.");
        }

        private object CrearSnapshotIncidencia(IncidenciaBE? i)
        {
            if (i == null) return new { };

            return new
            {
                i.IdIncidencia,
                i.IdRecorrido,
                i.IdConductor,
                i.ReportadoPor,
                i.CodigoIncidencia,
                i.TipoIncidencia,
                i.Descripcion,
                i.FechaHora,
                i.EstadoIncidencia,
                i.Prioridad,
                i.FechaCierre,
                i.Solucion,
                i.Estado
            };
        }
        private void RegistrarAuditoria(
        string accion,
        Guid? idRegistro,
        object? antes,
        object? despues,
        string observacion)
        {
            _auditoriaBC.RegistrarAutomatica(
                tabla: "Incidencia",
                idRegistro: idRegistro,
                accion: accion,
                datosAntes: antes,
                datosDespues: despues,
                modulo: "Operaciones",
                observacion: observacion
            );
        }
    }
}