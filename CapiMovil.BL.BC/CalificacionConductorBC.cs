using CapiMovil.BL.BE;
using CapiMovil.DL.DALC;

namespace CapiMovil.BL.BC
{
    public class CalificacionConductorBC
    {
        private readonly CalificacionConductorDALC _calificacionConductorDALC;

        public CalificacionConductorBC(CalificacionConductorDALC calificacionConductorDALC)
        {
            _calificacionConductorDALC = calificacionConductorDALC;
        }

        public bool Registrar(CalificacionConductorBE entidad)
        {
            if (entidad.IdPadre == Guid.Empty)
                throw new ArgumentException("Padre inválido.");
            if (entidad.IdConductor == Guid.Empty)
                throw new ArgumentException("Conductor inválido.");
            if (entidad.Puntaje < 1 || entidad.Puntaje > 5)
                throw new ArgumentException("La calificación debe estar entre 1 y 5 estrellas.");

            return _calificacionConductorDALC.Registrar(entidad);
        }
    }
}
