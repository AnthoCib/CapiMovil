using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapiMovil.BL.BE
{
    public class RutaBE
    {
        public Guid IdRuta { get; set; }
        public string CodigoRuta { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string Turno { get; set; } = "MANANA";
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public string? PuntoInicio { get; set; }
        public string? PuntoFin { get; set; }
        public string EstadoRuta { get; set; } = "ACTIVA";
        public bool Estado { get; set; } = true;

        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public DateTime? FechaEliminacion { get; set; }
    }
}
