using CapiMovil.BL.BE;
using CapiMovil.DL.DALC;

namespace CapiMovil.BL.BC
{
    public class ParaderoBC : ICrudBC<ParaderoBE>
    {
        private readonly ParaderoDALC _paraderoDALC;

        public ParaderoBC(ParaderoDALC paraderoDALC)
        {
            _paraderoDALC = paraderoDALC;
        }

        public List<ParaderoBE> Listar()
        {
            return _paraderoDALC.Listar();
        }

        public ParaderoBE? ListarPorId(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id de paradero inválido.");

            return _paraderoDALC.ListarPorId(id);
        }

        public bool Registrar(ParaderoBE entidad)
        {
            Validar(entidad);
            return _paraderoDALC.Registrar(entidad);
        }

        public bool Actualizar(ParaderoBE entidad)
        {
            if (entidad.IdParadero == Guid.Empty)
                throw new ArgumentException("Id de paradero inválido.");

            Validar(entidad);
            return _paraderoDALC.Actualizar(entidad);
        }

        public bool Eliminar(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id de paradero inválido.");

            return _paraderoDALC.Eliminar(id);
        }

        private static void Validar(ParaderoBE entidad)
        {
            if (entidad.IdRuta == Guid.Empty)
                throw new ArgumentException("Debe seleccionar una ruta.");

            if (string.IsNullOrWhiteSpace(entidad.Nombre))
                throw new ArgumentException("El nombre es obligatorio.");

            if (string.IsNullOrWhiteSpace(entidad.Direccion))
                throw new ArgumentException("La dirección es obligatoria.");

            if (entidad.OrdenParada <= 0)
                throw new ArgumentException("El orden de parada debe ser mayor a 0.");
        }
    }
}