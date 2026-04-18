using CapiMovil.BL.BE;
using CapiMovil.DL.DALC;

namespace CapiMovil.BL.BC
{
    public class RutaBC
    {
        private readonly RutaDALC _rutaDALC;

        public RutaBC(RutaDALC rutaDALC)
        {
            _rutaDALC = rutaDALC;
        }

        public List<RutaBE> Listar()
        {
            return _rutaDALC.Listar();
        }

        public RutaBE? ListarPorId(Guid idRuta)
        {
            if (idRuta == Guid.Empty)
                throw new ArgumentException("Id de ruta inválido.");

            return _rutaDALC.ListarPorId(idRuta);
        }

        public bool Registrar(RutaBE ruta)
        {
            Validar(ruta);
            return _rutaDALC.Registrar(ruta);
        }

        public bool Actualizar(RutaBE ruta)
        {
            if (ruta.IdRuta == Guid.Empty)
                throw new ArgumentException("Id de ruta inválido.");

            Validar(ruta);
            return _rutaDALC.Actualizar(ruta);
        }

        public bool Eliminar(Guid idRuta)
        {
            if (idRuta == Guid.Empty)
                throw new ArgumentException("Id de ruta inválido.");

            return _rutaDALC.Eliminar(idRuta);
        }

        private static void Validar(RutaBE ruta)
        {
            if (string.IsNullOrWhiteSpace(ruta.Nombre))
                throw new ArgumentException("El nombre de la ruta es obligatorio.");

            string turno = (ruta.Turno ?? "").Trim().ToUpperInvariant();
            if (turno != "MANANA" && turno != "TARDE" && turno != "NOCHE")
                throw new ArgumentException("El turno no es válido.");

            string estadoRuta = (ruta.EstadoRuta ?? "").Trim().ToUpperInvariant();
            if (estadoRuta != "ACTIVA" && estadoRuta != "INACTIVA" && estadoRuta != "SUSPENDIDA")
                throw new ArgumentException("El estado de la ruta no es válido.");

            if (ruta.HoraInicio >= ruta.HoraFin)
                throw new ArgumentException("La hora de inicio debe ser menor que la hora de fin.");
        }
    }
}