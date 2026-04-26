using CapiMovil.BL.BE;
using CapiMovil.DL.DALC;

namespace CapiMovil.BL.BC
{
    public class UbicacionBusBC : ICrudBC<UbicacionBusBE>
    {
        private readonly UbicacionBusDALC _ubicacionBusDALC;
        private readonly RecorridoBC _recorridoBC;

        public UbicacionBusBC(UbicacionBusDALC ubicacionBusDALC, RecorridoBC recorridoBC)
        {
            _ubicacionBusDALC = ubicacionBusDALC;
            _recorridoBC = recorridoBC;
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
            RecorridoBE? recorrido = _recorridoBC.ListarPorId(entidad.IdRecorrido);
            if (recorrido == null || !recorrido.Estado)
                throw new ArgumentException("El recorrido no existe o no está disponible.");

            if (!string.Equals(recorrido.EstadoRecorrido, "EN_CURSO", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Solo se puede registrar ubicación para recorridos EN_CURSO.");

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

            if (entidad.Latitud < -90m || entidad.Latitud > 90m)
                throw new ArgumentException("La latitud debe estar entre -90 y 90.");

            if (entidad.Longitud < -180m || entidad.Longitud > 180m)
                throw new ArgumentException("La longitud debe estar entre -180 y 180.");

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
