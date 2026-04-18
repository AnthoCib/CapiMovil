using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapiMovil.BL.BE
{
    public class BusBE
    {
        public Guid IdBus { get; set; }
        public string CodigoBus { get; set; } = string.Empty;
        public string Placa { get; set; } = string.Empty;
        public string? Marca { get; set; }
        public string? Modelo { get; set; }
        public string? Color { get; set; }
        public int? Anio { get; set; }
        public int Capacidad { get; set; }
        public string EstadoOperacion { get; set; } = "ACTIVO";
        public bool SeguroVigente { get; set; } = true;
        public DateTime? FechaVencimientoSOAT { get; set; }
        public DateTime? FechaRevisionTecnica { get; set; }
        public bool Estado { get; set; } = true;

        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public DateTime? FechaEliminacion { get; set; }
    }
}
