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

        public Guid IdPadre { get; set; }

        public string CodigoEstudiante { get; set; } = string.Empty;

        public string Nombres { get; set; } = string.Empty;

        public string ApellidoPaterno { get; set; } = string.Empty;

        public string ApellidoMaterno { get; set; } = string.Empty;

        public string? DNI { get; set; }

        public DateTime? FechaNacimiento { get; set; }

        public string? Genero { get; set; }

        public string? Grado { get; set; }

        public string? Seccion { get; set; }

        public string? Direccion { get; set; }

        public bool Estado { get; set; }

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