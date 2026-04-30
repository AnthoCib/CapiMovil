using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapiMovil.BL.BE
{
    public class ParaderoConductorBE
    {
        public Guid IdParadero { get; set; }

        public Guid IdRuta { get; set; }

        public string CodigoParadero { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public string Direccion { get; set; } = string.Empty;

        public int OrdenParada { get; set; }

        public TimeSpan? HoraEstimada { get; set; }

        public int TotalAlumnos { get; set; }

        public int TotalRegistrados { get; set; }

        public bool Completado { get; set; }
    }
}