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

          
            return _dalc.Registrar(entidad);
        }

        public bool Actualizar(EstudianteBE entidad)
        {
            if (entidad.IdEstudiante == Guid.Empty)
                throw new ArgumentException("Id inválido.");

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
    }
}