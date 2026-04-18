using CapiMovil.BL.BE;
using CapiMovil.DL.DALC;

namespace CapiMovil.BL.BC
{
    public class UbicacionBusBC : ICrudBC<UbicacionBusBE>
    {
        private readonly UbicacionBusDALC _ubicacionBusDALC;

        public UbicacionBusBC(UbicacionBusDALC ubicacionBusDALC)
        {
            _ubicacionBusDALC = ubicacionBusDALC;
        }

        public List<UbicacionBusBE> Listar()
        {
            return _ubicacionBusDALC.Listar();
        }

        public UbicacionBusBE? ListarPorId(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id de ubicación inválido.");

            return _ubicacionBusDALC.ListarPorId(id);
        }

        public bool Registrar(UbicacionBusBE entidad)
        {
            Validar(entidad);
            return _ubicacionBusDALC.Registrar(entidad);
        }

        public bool Actualizar(UbicacionBusBE entidad)
        {
            if (entidad.IdUbicacion == Guid.Empty)
                throw new ArgumentException("Id de ubicación inválido.");

            Validar(entidad);
            return _ubicacionBusDALC.Actualizar(entidad);
        }

        public bool Eliminar(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id de ubicación inválido.");

            return _ubicacionBusDALC.Eliminar(id);
        }

        private static void Validar(UbicacionBusBE entidad)
        {
            if (entidad.IdRecorrido == Guid.Empty)
                throw new ArgumentException("Debe seleccionar un recorrido.");

            string fuente = (entidad.Fuente ?? "").Trim().ToUpperInvariant();
            if (!string.IsNullOrWhiteSpace(fuente) &&
                fuente != "GPS" &&
                fuente != "MANUAL" &&
                fuente != "API")
            {
                throw new ArgumentException("La fuente no es válida.");
            }
        }
    }
}