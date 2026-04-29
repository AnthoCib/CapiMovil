using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapiMovil.BL.BE
{
    public class EstudianteRutaEstadoBE
    {
        public Guid IdEstudiante { get; set; }

        public string Nombres { get; set; } = string.Empty;

        public string ApellidoPaterno { get; set; } = string.Empty;

        public string ApellidoMaterno { get; set; } = string.Empty;

        public string Grado { get; set; } = string.Empty;

        public string Seccion { get; set; } = string.Empty;

        public string EstadoEvento { get; set; } = "PENDIENTE";

        public string NombrePadre { get; set; } = string.Empty;

        public string? TelefonoPadre { get; set; }

        public string? ParaderoSubidaNombre { get; set; }

        public string? ParaderoSubidaDireccion { get; set; }

        public string? ParaderoBajadaNombre { get; set; }

        public string? ParaderoBajadaDireccion { get; set; }

        public string NombreCompleto
        {
            get
            {
                return $"{Nombres} {ApellidoPaterno} {ApellidoMaterno}".Trim();
            }
        }
    }
}
