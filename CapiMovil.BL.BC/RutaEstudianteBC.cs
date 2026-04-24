using CapiMovil.BL.BE;
using CapiMovil.DL.DALC;

namespace CapiMovil.BL.BC
{
    public class RutaEstudianteBC : ICrudBC<RutaEstudianteBE>
    {
        private readonly RutaEstudianteDALC _rutaEstudianteDALC;

        public RutaEstudianteBC(RutaEstudianteDALC rutaEstudianteDALC)
        {
            _rutaEstudianteDALC = rutaEstudianteDALC;
        }

        public List<RutaEstudianteBE> Listar()
        {
            return _rutaEstudianteDALC.Listar();
        }

        public RutaEstudianteBE? ListarPorId(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id de asignación inválido.");

            return _rutaEstudianteDALC.ListarPorId(id);
        }

        public bool Registrar(RutaEstudianteBE entidad)
        {
            Validar(entidad);
            return _rutaEstudianteDALC.Registrar(entidad);
        }

        public bool Actualizar(RutaEstudianteBE entidad)
        {
            if (entidad.IdRutaEstudiante == Guid.Empty)
                throw new ArgumentException("Id de asignación inválido.");

            Validar(entidad);
            return _rutaEstudianteDALC.Actualizar(entidad);
        }

        public bool Eliminar(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id de asignación inválido.");

            return _rutaEstudianteDALC.Eliminar(id);
        }

        private static void Validar(RutaEstudianteBE entidad)
        {
            if (entidad.IdRuta == Guid.Empty)
                throw new ArgumentException("Debe seleccionar una ruta.");

            if (entidad.IdEstudiante == Guid.Empty)
                throw new ArgumentException("Debe seleccionar un estudiante.");

            if (entidad.FechaFinVigencia.HasValue &&
                entidad.FechaFinVigencia.Value.Date < entidad.FechaInicioVigencia.Date)
            {
                throw new ArgumentException("La fecha fin no puede ser menor que la fecha inicio.");
            }

            string estadoAsignacion = (entidad.EstadoAsignacion ?? string.Empty).Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(estadoAsignacion))
                estadoAsignacion = entidad.Estado ? "ACTIVO" : "INACTIVO";

            if (estadoAsignacion != "ACTIVO" && estadoAsignacion != "INACTIVO")
                throw new ArgumentException("El estado de asignación no es válido.");

            entidad.EstadoAsignacion = estadoAsignacion;
        }
    }
}