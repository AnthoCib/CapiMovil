using CapiMovil.BL.BE;
using CapiMovil.DL.DALC;

namespace CapiMovil.BL.BC
{
    public class EstudianteBC:ICrudBC<EstudianteBE>
    {
        private readonly EstudianteDALC _dalc;

        public EstudianteBC(EstudianteDALC dalc)
        {
            _dalc = dalc;
        }

        public List<EstudianteBE> Listar()
        {
            return _dalc.Listar();
        }

        public EstudianteBE? ListarPorId(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id inválido.");

            return _dalc.ListarPorId(id);
        }

        public bool Registrar(EstudianteBE entidad)
        {
            if (entidad.IdPadre == Guid.Empty)
                throw new ArgumentException("Debe seleccionar un padre.");

            if (string.IsNullOrWhiteSpace(entidad.Nombres))
                throw new ArgumentException("Nombres obligatorios.");

            ValidarCoordenadas(entidad.LatitudCasa, entidad.LongitudCasa);

            return _dalc.Registrar(entidad);
        }

        public bool Actualizar(EstudianteBE entidad)
        {
            if (entidad.IdEstudiante == Guid.Empty)
                throw new ArgumentException("Id inválido.");

            if (string.IsNullOrWhiteSpace(entidad.CodigoEstudiante))
            {
                EstudianteBE? actual = _dalc.ListarPorId(entidad.IdEstudiante);
                entidad.CodigoEstudiante = actual?.CodigoEstudiante ?? string.Empty;
            }

            ValidarCoordenadas(entidad.LatitudCasa, entidad.LongitudCasa);

            return _dalc.Actualizar(entidad);
        }

        public bool Eliminar(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id inválido.");

            return _dalc.Eliminar(id);
        }

        public Guid ObtenerPadrePorEstudiante(Guid idEstudiante)
        {
            if (idEstudiante == Guid.Empty)
                throw new ArgumentException("El id del estudiante es inválido.");

            return _dalc.ObtenerPadrePorEstudiante(idEstudiante);
        }

        private static void ValidarCoordenadas(decimal? latitud, decimal? longitud)
        {
            if (latitud.HasValue && (latitud < -90m || latitud > 90m))
                throw new ArgumentException("La latitud de casa debe estar entre -90 y 90.");

            if (longitud.HasValue && (longitud < -180m || longitud > 180m))
                throw new ArgumentException("La longitud de casa debe estar entre -180 y 180.");
        }
    }
}
