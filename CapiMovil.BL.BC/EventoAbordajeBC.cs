using CapiMovil.BL.BE;
using CapiMovil.DL.DALC;

namespace CapiMovil.BL.BC
{
    public class EventoAbordajeBC : ICrudBC<EventoAbordajeBE>
    {
        private readonly EventoAbordajeDALC _eventoAbordajeDALC;
        private readonly NotificacionBC _notificacionBC;
        private readonly EstudianteBC _estudianteBC;
        public EventoAbordajeBC(
              EventoAbordajeDALC eventoAbordajeDALC,
              NotificacionBC notificacionBC,
              EstudianteBC estudianteBC)
        {
            _eventoAbordajeDALC = eventoAbordajeDALC;
            _notificacionBC = notificacionBC;
            _estudianteBC = estudianteBC;
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
            ValidarSecuenciaAbordaje(entidad);

            bool ok = _eventoAbordajeDALC.Registrar(entidad);

            if (ok)
            {
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

                    if (!string.IsNullOrWhiteSpace(titulo))
                    {
                        _notificacionBC.RegistrarAutomatica(
                            idPadre,
                            entidad.IdEstudiante,
                            titulo,
                            mensaje,
                            tipo,
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
            return _eventoAbordajeDALC.Actualizar(entidad);
        }

        public bool Eliminar(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id de evento inválido.");

            return _eventoAbordajeDALC.Eliminar(id);
        }

        private static void Validar(EventoAbordajeBE entidad)
        {
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

        private void ValidarSecuenciaAbordaje(EventoAbordajeBE entidad)
        {
            EventoAbordajeResumenBE resumen = _eventoAbordajeDALC.ObtenerResumenPorEstudianteRecorrido(entidad.IdRecorrido, entidad.IdEstudiante);
            string tipo = (entidad.TipoEvento ?? string.Empty).Trim().ToUpperInvariant();

            bool tieneSubida = resumen.TotalSubidas > 0;
            bool tieneBajada = resumen.TotalBajadas > 0;
            bool marcadoAusente = resumen.TotalAusentes > 0;
            bool marcadoNoAbordo = resumen.TotalNoAbordo > 0;
            bool marcadoNoAbordaje = marcadoAusente || marcadoNoAbordo;

            if (tipo == "SUBIDA")
            {
                if (marcadoAusente)
                    throw new ArgumentException("No se puede registrar este evento porque ya fue marcado como ausente.");

                if (marcadoNoAbordo)
                    throw new ArgumentException("No se puede registrar este evento porque ya fue marcado como no abordó.");

                if (tieneSubida && !tieneBajada)
                    throw new ArgumentException("El alumno ya registró una subida en este recorrido.");

                if (tieneSubida && tieneBajada)
                    throw new ArgumentException("El alumno ya completó su ciclo de abordaje en este recorrido.");
            }
            else if (tipo == "BAJADA")
            {
                if (marcadoAusente)
                    throw new ArgumentException("No se puede registrar este evento porque ya fue marcado como ausente.");

                if (marcadoNoAbordo)
                    throw new ArgumentException("No se puede registrar este evento porque ya fue marcado como no abordó.");

                if (!tieneSubida)
                    throw new ArgumentException("No se puede registrar la bajada porque el alumno aún no tiene una subida.");

                if (tieneBajada)
                    throw new ArgumentException("El alumno ya registró una bajada en este recorrido.");
            }
            else if (tipo == "AUSENTE")
            {
                if (tieneSubida || tieneBajada)
                    throw new ArgumentException("No se puede registrar este evento porque el alumno ya tiene eventos de abordaje en este recorrido.");

                if (marcadoAusente)
                    throw new ArgumentException("El alumno ya fue marcado como ausente en este recorrido.");

                if (marcadoNoAbordo)
                    throw new ArgumentException("No se puede registrar ausente porque el alumno ya fue marcado como no abordó.");
            }
            else if (tipo == "NO_ABORDO")
            {
                if (tieneSubida || tieneBajada)
                    throw new ArgumentException("No se puede registrar este evento porque el alumno ya tiene eventos de abordaje en este recorrido.");

                if (marcadoNoAbordo)
                    throw new ArgumentException("El alumno ya fue marcado como no abordó en este recorrido.");

                if (marcadoAusente)
                    throw new ArgumentException("No se puede registrar no abordó porque el alumno ya fue marcado como ausente.");
            }

            if (resumen.TotalEventos >= 2 && (tipo == "SUBIDA" || tipo == "BAJADA"))
                throw new ArgumentException("El alumno ya completó su ciclo de abordaje en este recorrido.");

            if (marcadoNoAbordaje && (tipo == "SUBIDA" || tipo == "BAJADA"))
                throw new ArgumentException("No se puede registrar este evento porque el alumno tiene un estado de no abordaje en este recorrido.");
        }
    }
}
