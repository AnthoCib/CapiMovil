using CapiMovil.BL.BE;
using CapiMovil.DL.DALC;

namespace CapiMovil.BL.BC
{
    public class RolBC : ICrudBC<RolBE>
    {
        private readonly RolDALC _rolDALC;

        public RolBC(RolDALC rolDALC)
        {
            _rolDALC = rolDALC;
        }

        public List<RolBE> Listar()
        {
            return _rolDALC.Listar();
        }

        
        public RolBE? ListarPorId(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id inválido.");

            return _rolDALC.ListarPorId(id);
        }

        public bool Registrar(RolBE entidad)
        {
            if (string.IsNullOrWhiteSpace(entidad.Nombre))
                throw new ArgumentException("El nombre del rol es obligatorio.");

            return _rolDALC.Registrar(entidad);
        }

        public bool Actualizar(RolBE entidad)
        {
            if (entidad.IdRol == Guid.Empty)
                throw new ArgumentException("Id inválido.");

            if (string.IsNullOrWhiteSpace(entidad.Nombre))
                throw new ArgumentException("El nombre del rol es obligatorio.");

            return _rolDALC.Actualizar(entidad);
        }

        public bool Eliminar(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id inválido.");

            return _rolDALC.Eliminar(id);
        }
    }
}