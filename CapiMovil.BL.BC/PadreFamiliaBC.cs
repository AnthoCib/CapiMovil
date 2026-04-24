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


        public PadreFamiliaBE? ObtenerPorIdUsuario(Guid idUsuario)
        {
            if (idUsuario == Guid.Empty)
                throw new ArgumentException("Id de usuario inválido.");

            return _dalc.ObtenerPorIdUsuario(idUsuario);
        }

        public List<UsuarioBE> ListarUsuariosDisponibles()
        {
            return _dalc.ListarUsuariosDisponibles();
        }

        public List<PadreFamiliaBE> ListarParaCombo()
        {
            return _dalc.ListarParaCombo();
        }

        public bool Registrar(PadreFamiliaBE entidad)
        {
            ValidarCamposObligatorios(entidad);

            if (_dalc.ExistePorIdUsuario(entidad.IdUsuario))
                throw new ArgumentException("El usuario seleccionado ya está vinculado a un padre de familia.");

            if (!string.IsNullOrWhiteSpace(entidad.DNI) && _dalc.ExistePorDni(entidad.DNI))
                throw new ArgumentException("El DNI ingresado ya está registrado para otro padre de familia.");

            NormalizarTexto(entidad);
            return _dalc.Registrar(entidad);
        }

        public bool Actualizar(PadreFamiliaBE entidad)
        {
            if (entidad == null)
                throw new ArgumentNullException(nameof(entidad));

            if (entidad.IdPadre == Guid.Empty)
                throw new ArgumentException("Id inválido.");

            ValidarCamposObligatorios(entidad);

            if (_dalc.ExistePorIdUsuario(entidad.IdUsuario, entidad.IdPadre))
                throw new ArgumentException("El usuario seleccionado ya está vinculado a otro padre de familia.");

            if (!string.IsNullOrWhiteSpace(entidad.DNI) && _dalc.ExistePorDni(entidad.DNI, entidad.IdPadre))
                throw new ArgumentException("El DNI ingresado ya está registrado para otro padre de familia.");

            NormalizarTexto(entidad);
            return _dalc.Actualizar(entidad);
        }

        public bool Eliminar(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id inválido.");

            return _dalc.Eliminar(id);
        }

        private static void ValidarCamposObligatorios(PadreFamiliaBE entidad)
        {
            if (entidad == null)
                throw new ArgumentNullException(nameof(entidad));

            if (entidad.IdUsuario == Guid.Empty)
                throw new ArgumentException("Debe seleccionar un usuario.");

            if (string.IsNullOrWhiteSpace(entidad.Nombres))
                throw new ArgumentException("Los nombres son obligatorios.");

            if (string.IsNullOrWhiteSpace(entidad.ApellidoPaterno))
                throw new ArgumentException("El apellido paterno es obligatorio.");

            if (string.IsNullOrWhiteSpace(entidad.ApellidoMaterno))
                throw new ArgumentException("El apellido materno es obligatorio.");

            if (!string.IsNullOrWhiteSpace(entidad.DNI) && entidad.DNI.Trim().Length != 8)
                throw new ArgumentException("El DNI debe tener 8 dígitos.");
        }

        private static void NormalizarTexto(PadreFamiliaBE entidad)
        {
            entidad.Nombres = entidad.Nombres.Trim();
            entidad.ApellidoPaterno = entidad.ApellidoPaterno.Trim();
            entidad.ApellidoMaterno = entidad.ApellidoMaterno.Trim();
            entidad.DNI = string.IsNullOrWhiteSpace(entidad.DNI) ? null : entidad.DNI.Trim();
            entidad.Telefono = string.IsNullOrWhiteSpace(entidad.Telefono) ? null : entidad.Telefono.Trim();
            entidad.TelefonoAlterno = string.IsNullOrWhiteSpace(entidad.TelefonoAlterno) ? null : entidad.TelefonoAlterno.Trim();
            entidad.Direccion = string.IsNullOrWhiteSpace(entidad.Direccion) ? null : entidad.Direccion.Trim();
            entidad.CorreoContacto = string.IsNullOrWhiteSpace(entidad.CorreoContacto)
                ? null
                : entidad.CorreoContacto.Trim().ToLowerInvariant();
        }
    }
}
