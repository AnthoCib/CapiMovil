using CapiMovil.BL.BE;
using CapiMovil.DL.DALC;

namespace CapiMovil.BL.BC
{
    public class PadreFamiliaBC : ICrudBC<PadreFamiliaBE>
    {
        private readonly PadreFamiliaDALC _dalc;

        public PadreFamiliaBC(PadreFamiliaDALC dalc)
        {
            _dalc = dalc;
        }

        public List<PadreFamiliaBE> Listar()
        {
            return _dalc.Listar();
        }

        public PadreFamiliaBE? ListarPorId(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id inválido.");

            return _dalc.ListarPorId(id);
        }
        public List<UsuarioBE> ListarUsuariosDisponibles()
        {
            return _dalc.ListarUsuariosDisponibles();
        }

        public bool Registrar(PadreFamiliaBE entidad)
        {
            if (entidad == null)
                throw new ArgumentNullException(nameof(entidad));

            if (entidad.IdUsuario == Guid.Empty)
                throw new ArgumentException("Debe seleccionar un usuario.");

            if (_dalc.ExistePorIdUsuario(entidad.IdUsuario))
                throw new ArgumentException("El usuario seleccionado ya está vinculado a un padre de familia.");

            return _dalc.Registrar(entidad);
        }

        public bool Actualizar(PadreFamiliaBE entidad)
        {
            if (entidad.IdPadre == Guid.Empty)
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