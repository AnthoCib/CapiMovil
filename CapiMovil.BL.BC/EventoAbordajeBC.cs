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
    }
}