using CapiMovil.BL.BE;
using CapiMovil.DL.DALC;

namespace CapiMovil.BL.BC
{
    public class RutaBC : ICrudBC<RutaBE>
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

            ValidarCoordenadas(ruta.LatitudInicio, ruta.LongitudInicio, "inicio");
            ValidarCoordenadas(ruta.LatitudFin, ruta.LongitudFin, "fin");

            if ((ruta.LatitudInicio.HasValue ^ ruta.LongitudInicio.HasValue) ||
                (ruta.LatitudFin.HasValue ^ ruta.LongitudFin.HasValue))
            {
                throw new ArgumentException("Debe registrar latitud y longitud completas para cada punto.");
            }

            bool tieneInicio = ruta.LatitudInicio.HasValue && ruta.LongitudInicio.HasValue;
            bool tieneFin = ruta.LatitudFin.HasValue && ruta.LongitudFin.HasValue;

            if (tieneInicio ^ tieneFin)
                throw new ArgumentException("Debe seleccionar tanto el punto de inicio como el punto de fin.");
        }

        private static void ValidarCoordenadas(decimal? latitud, decimal? longitud, string etiqueta)
        {
            if (latitud.HasValue && (latitud < -90m || latitud > 90m))
                throw new ArgumentException($"La latitud de {etiqueta} debe estar entre -90 y 90.");

            if (longitud.HasValue && (longitud < -180m || longitud > 180m))
                throw new ArgumentException($"La longitud de {etiqueta} debe estar entre -180 y 180.");
        }
    }
}
