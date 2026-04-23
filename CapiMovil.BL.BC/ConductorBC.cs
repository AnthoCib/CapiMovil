using CapiMovil.BL.BE;
using CapiMovil.DL.DALC;

namespace CapiMovil.BL.BC
{
    public class ConductorBC : ICrudBC<ConductorBE>
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
            ValidarCamposObligatorios(entidad);

            if (_dalc.ExistePorIdUsuario(entidad.IdUsuario))
                throw new ArgumentException("El usuario seleccionado ya está vinculado a un conductor.");

            if (!string.IsNullOrWhiteSpace(entidad.DNI) && _dalc.ExistePorDni(entidad.DNI))
                throw new ArgumentException("El DNI ingresado ya está registrado para otro conductor.");

            if (_dalc.ExistePorLicencia(entidad.Licencia))
                throw new ArgumentException("La licencia ingresada ya está registrada para otro conductor.");

            NormalizarTexto(entidad);
            return _dalc.Registrar(entidad);
        }

        public bool Actualizar(ConductorBE entidad)
        {
            if (entidad == null)
                throw new ArgumentNullException(nameof(entidad));

            if (entidad.IdConductor == Guid.Empty)
                throw new ArgumentException("Id inválido.");

            ValidarCamposObligatorios(entidad);

            if (_dalc.ExistePorIdUsuario(entidad.IdUsuario, entidad.IdConductor))
                throw new ArgumentException("El usuario seleccionado ya está vinculado a otro conductor.");

            if (!string.IsNullOrWhiteSpace(entidad.DNI) && _dalc.ExistePorDni(entidad.DNI, entidad.IdConductor))
                throw new ArgumentException("El DNI ingresado ya está registrado para otro conductor.");

            if (_dalc.ExistePorLicencia(entidad.Licencia, entidad.IdConductor))
                throw new ArgumentException("La licencia ingresada ya está registrada para otro conductor.");

            NormalizarTexto(entidad);
            return _dalc.Actualizar(entidad);
        }

        public bool Eliminar(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id inválido.");

            return _dalc.Eliminar(id);
        }

        private static void ValidarCamposObligatorios(ConductorBE entidad)
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

            if (string.IsNullOrWhiteSpace(entidad.Licencia))
                throw new ArgumentException("La licencia es obligatoria.");
        }

        private static void NormalizarTexto(ConductorBE entidad)
        {
            entidad.Nombres = entidad.Nombres.Trim();
            entidad.ApellidoPaterno = entidad.ApellidoPaterno.Trim();
            entidad.ApellidoMaterno = entidad.ApellidoMaterno.Trim();
            entidad.DNI = string.IsNullOrWhiteSpace(entidad.DNI) ? null : entidad.DNI.Trim();
            entidad.Licencia = entidad.Licencia.Trim().ToUpperInvariant();
            entidad.CategoriaLicencia = string.IsNullOrWhiteSpace(entidad.CategoriaLicencia) ? null : entidad.CategoriaLicencia.Trim().ToUpperInvariant();
            entidad.Telefono = string.IsNullOrWhiteSpace(entidad.Telefono) ? null : entidad.Telefono.Trim();
            entidad.Direccion = string.IsNullOrWhiteSpace(entidad.Direccion) ? null : entidad.Direccion.Trim();
        }
    }
}
