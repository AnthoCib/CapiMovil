using CapiMovil.BL.BE;
using CapiMovil.DL.DALC;

namespace CapiMovil.BL.BC
{
    public class ConductorBC :ICrudBC<ConductorBE>
    {
        private readonly ConductorDALC _dalc;

        public ConductorBC(ConductorDALC dalc)
        {
            _dalc = dalc;
        }

        public List<ConductorBE> Listar()
        {
            return _dalc.Listar();
        }

        public ConductorBE? ListarPorId(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id inválido.");

            return _dalc.ListarPorId(id);
        }


        public List<UsuarioBE> ListarUsuariosDisponibles()
        {
            return _dalc.ListarUsuariosDisponibles();
        }

        public bool Registrar(ConductorBE entidad)
        {
            if (entidad == null)
                throw new ArgumentNullException(nameof(entidad));

            if (entidad.IdUsuario == Guid.Empty)
                throw new ArgumentException("Debe seleccionar un usuario.");

            if (_dalc.ExistePorIdUsuario(entidad.IdUsuario))
                throw new ArgumentException("El usuario seleccionado ya está vinculado a un conductor.");

            return _dalc.Registrar(entidad);
        }

        public bool Actualizar(ConductorBE entidad)
        {
            if (entidad.IdConductor == Guid.Empty)
                throw new ArgumentException("Id inválido.");

            return _dalc.Actualizar(entidad);
        }

        public bool Eliminar(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id inválido.");

            return _dalc.Eliminar(id);
        }

     


      
    }
}