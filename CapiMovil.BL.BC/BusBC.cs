using CapiMovil.BL.BE;
using CapiMovil.DL.DALC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            if (string.IsNullOrWhiteSpace(bus.Placa))
                throw new ArgumentException("La placa es obligatoria.");

            if (bus.Capacidad <= 0)
                throw new ArgumentException("La capacidad debe ser mayor a 0.");

            string estado = (bus.EstadoOperacion ?? "").Trim().ToUpperInvariant();
            if (estado != "ACTIVO" && estado != "INACTIVO" && estado != "MANTENIMIENTO")
                throw new ArgumentException("Estado operativo inválido.");

            return _busDALC.Registrar(bus);
        }
        private static void ValidarBus(BusBE bus)
        {
            if (string.IsNullOrWhiteSpace(bus.Placa))
                throw new ArgumentException("La placa es obligatoria.");

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
        }
    }
}
