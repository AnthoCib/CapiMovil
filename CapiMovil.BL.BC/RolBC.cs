using CapiMovil.BL.BE;
using CapiMovil.DL.DALC;

namespace CapiMovil.BL.BC
{
    public class RolBC : ICrudBC<RolBE>
    {
        private readonly RolDALC _rolDALC;
        private readonly AuditoriaBC _auditoriaBC;

        public RolBC(RolDALC rolDALC, AuditoriaBC auditoriaBC)
        {
            _rolDALC = rolDALC;
            _auditoriaBC = auditoriaBC;
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
            Validar(entidad, true);

            bool ok = _rolDALC.Registrar(entidad);

            if (ok)
            {
                RegistrarAuditoria(
                    "INSERT",
                    entidad.IdRol == Guid.Empty ? null : entidad.IdRol,
                    null,
                    CrearSnapshotRol(entidad),
                    "Se registró el rol"
                );
            }

            return ok;
        }

        public bool Actualizar(RolBE entidad)
        {
            if (entidad.IdRol == Guid.Empty)
                throw new ArgumentException("Id inválido.");

            Validar(entidad, false);

            var antes = _rolDALC.ListarPorId(entidad.IdRol);
            bool ok = _rolDALC.Actualizar(entidad);

            if (ok)
            {
                var despues = _rolDALC.ListarPorId(entidad.IdRol);

                RegistrarAuditoria(
                    "UPDATE",
                    entidad.IdRol,
                    CrearSnapshotRol(antes),
                    CrearSnapshotRol(despues),
                    "Se actualizó el rol"
                );
            }

            return ok;
        }

        public bool Eliminar(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id inválido.");

            var antes = _rolDALC.ListarPorId(id);
            bool ok = _rolDALC.Eliminar(id);

            if (ok)
            {
                RegistrarAuditoria(
                    "DELETE",
                    id,
                    CrearSnapshotRol(antes),
                    null,
                    "Se eliminó lógicamente el rol"
                );
            }

            return ok;
        }

        private static void Validar(RolBE entidad, bool esNuevo)
        {
            if (entidad == null)
                throw new ArgumentNullException(nameof(entidad));

            if (!esNuevo && entidad.IdRol == Guid.Empty)
                throw new ArgumentException("Id inválido.");

            if (string.IsNullOrWhiteSpace(entidad.Nombre))
                throw new ArgumentException("El nombre del rol es obligatorio.");

            entidad.Nombre = entidad.Nombre.Trim();

            if (entidad.Nombre.Length > 100)
                throw new ArgumentException("El nombre del rol no puede superar los 100 caracteres.");
        }

        private void RegistrarAuditoria(
            string accion,
            Guid? idRegistro,
            object? antes,
            object? despues,
            string observacion)
        {
            _auditoriaBC.RegistrarAutomatica(
                tabla: "Rol",
                idRegistro: idRegistro,
                accion: accion,
                datosAntes: antes,
                datosDespues: despues,
                modulo: "Seguridad",
                observacion: observacion
            );
        }

        private object CrearSnapshotRol(RolBE? r)
        {
            if (r == null) return new { };

            return new
            {
                r.IdRol,
                r.CodigoRol,
                r.Nombre,
                r.Estado
            };
        }
    }
}