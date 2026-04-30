using CapiMovil.BL.BE;
using CapiMovil.DL.DALC;

namespace CapiMovil.BL.BC
{
    public class BusBC : ICrudBC<BusBE>
    {
        private readonly BusDALC _busDALC;

        public BusBC(BusDALC busDALC)
        {
            _busDALC = busDALC;
        }

        public bool Actualizar(BusBE entidad)
        {
            if (entidad.IdBus == Guid.Empty)
                throw new ArgumentException("Id de bus inválido.");

            NormalizarBus(entidad);
            ValidarBus(entidad);
            return _busDALC.Actualizar(entidad);
        }

        public bool Eliminar(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id de bus inválido.");

            return _busDALC.Eliminar(id);
        }

        public List<BusBE> Listar()
        {
            return _busDALC.Listar();
        }

        public BusBE? ListarPorId(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id de bus inválido.");

            return _busDALC.ListarPorId(id);
        }

        public bool Registrar(BusBE bus)
        {
            NormalizarBus(bus);
            ValidarBus(bus);
            return _busDALC.Registrar(bus);
        }

        private static void ValidarBus(BusBE bus)
        {
            if (string.IsNullOrWhiteSpace(bus.Placa))
                throw new ArgumentException("La placa es obligatoria.");

            if (string.IsNullOrWhiteSpace(bus.Marca))
                throw new ArgumentException("La marca es obligatoria para generar un código de bus consistente.");

            if (bus.Capacidad <= 0)
                throw new ArgumentException("La capacidad debe ser mayor a 0.");

            string estadoOperacion = (bus.EstadoOperacion ?? "").Trim().ToUpperInvariant();
            if (estadoOperacion != "ACTIVO" &&
                estadoOperacion != "INACTIVO" &&
                estadoOperacion != "MANTENIMIENTO")
            {
                throw new ArgumentException("El estado operativo no es válido.");
            }

            if (bus.Anio.HasValue && (bus.Anio < 1900 || bus.Anio > 2100))
                throw new ArgumentException("El año ingresado no es válido.");

            if (bus.FechaVencimientoSOAT.HasValue)
            {
                DateTime fechaSoat = bus.FechaVencimientoSOAT.Value.Date;
                bool soatVigentePorFecha = fechaSoat >= DateTime.Today;

                if (bus.SeguroVigente && !soatVigentePorFecha)
                    throw new ArgumentException("SOAT vencido: no se puede marcar el seguro como vigente.");

                if (!bus.SeguroVigente && soatVigentePorFecha)
                    throw new ArgumentException("SOAT vigente por fecha: marque el seguro como vigente o ajuste la fecha de vencimiento.");
            }
        }

        private static void NormalizarBus(BusBE bus)
        {
            bus.Placa = (bus.Placa ?? string.Empty).Trim().ToUpperInvariant();
            bus.Marca = string.IsNullOrWhiteSpace(bus.Marca) ? null : bus.Marca.Trim().ToUpperInvariant();
            bus.Modelo = string.IsNullOrWhiteSpace(bus.Modelo) ? null : bus.Modelo.Trim();
            bus.Color = string.IsNullOrWhiteSpace(bus.Color) ? null : bus.Color.Trim();
            bus.EstadoOperacion = (bus.EstadoOperacion ?? string.Empty).Trim().ToUpperInvariant();
        }
    }
}
