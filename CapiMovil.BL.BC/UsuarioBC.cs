using CapiMovil.BL.BE;
using CapiMovil.DL.DALC;

namespace CapiMovil.BL.BC
{
    public class UsuarioBC :ICrudBC<UsuarioBE>
    {
        private readonly UsuarioDALC _usuarioDALC;

        public UsuarioBC(UsuarioDALC usuarioDALC)
        {
            _usuarioDALC = usuarioDALC;
        }

        public List<UsuarioBE> Listar()
        {
            return _usuarioDALC.Listar();
        }

        public UsuarioBE? ListarPorId(Guid idUsuario)
        {
            return _usuarioDALC.ListarPorId(idUsuario);
        }

        public bool Registrar(UsuarioBE usuario)
        {
            if (usuario == null)
                throw new ArgumentNullException(nameof(usuario));

            if (usuario.IdRol == Guid.Empty)
                throw new ArgumentException("El rol es obligatorio.");

            if (string.IsNullOrWhiteSpace(usuario.Username))
                throw new ArgumentException("El nombre de usuario es obligatorio.");

            if (string.IsNullOrWhiteSpace(usuario.Correo))
                throw new ArgumentException("El correo es obligatorio.");

            if (string.IsNullOrWhiteSpace(usuario.PasswordHash))
                throw new ArgumentException("La contraseña es obligatoria.");

        
            usuario.Username = usuario.Username.Trim();
            usuario.Correo = usuario.Correo.Trim().ToLower();

            // Aquí se hashea la contraseña
            usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(usuario.PasswordHash);

            return _usuarioDALC.Registrar(usuario);
        }
        public bool VerificarPassword(string passwordPlano, string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(passwordPlano) || string.IsNullOrWhiteSpace(passwordHash))
                return false;

            return BCrypt.Net.BCrypt.Verify(passwordPlano, passwordHash);
        }

        public bool Actualizar(UsuarioBE usuario)
        {
            if (usuario.IdUsuario == Guid.Empty)
                throw new ArgumentException("Id inválido.");

            return _usuarioDALC.Actualizar(usuario);
        }

        public bool Eliminar(Guid idUsuario)
        {
            return _usuarioDALC.Eliminar(idUsuario);
        }

        public bool CambiarPassword(Guid idUsuario, string passwordNueva, string confirmarPassword)
        {
            if (idUsuario == Guid.Empty)
                throw new ArgumentException("El usuario es inválido.");

            if (string.IsNullOrWhiteSpace(passwordNueva))
                throw new ArgumentException("La nueva contraseña es obligatoria.");

            if (string.IsNullOrWhiteSpace(confirmarPassword))
                throw new ArgumentException("La confirmación de contraseña es obligatoria.");

            if (passwordNueva != confirmarPassword)
                throw new ArgumentException("Las contraseñas no coinciden.");

            if (passwordNueva.Length < 6)
                throw new ArgumentException("La contraseña debe tener al menos 8 caracteres.");

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(passwordNueva);

            return _usuarioDALC.CambiarPassword(idUsuario, passwordHash);
        }


        public UsuarioBE? Login(string usuarioOCorreo, string password)
        {
            if (string.IsNullOrWhiteSpace(usuarioOCorreo))
                throw new ArgumentException("Ingrese su usuario o correo.");

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Ingrese su contraseña.");

            UsuarioBE? usuario = _usuarioDALC.ObtenerPorUsuarioOCorreo(usuarioOCorreo.Trim());

            if (usuario == null)
                return null;

            if (!usuario.Estado)
                return null;

            bool claveValida = BCrypt.Net.BCrypt.Verify(password, usuario.PasswordHash);

            if (!claveValida)
                return null;

            return usuario;
        }
    }
}