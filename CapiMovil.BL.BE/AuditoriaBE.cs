using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapiMovil.BL.BE
{
    public class AuditoriaBE
    {
        public Guid IdAuditoria { get; set; }
        public string CodigoAuditoria { get; set; } = string.Empty;
        public string Tabla { get; set; } = string.Empty;
        public Guid? IdRegistro { get; set; }
        public string Accion { get; set; } = string.Empty;
        public string? DatosAntes { get; set; }
        public string? DatosDespues { get; set; }
        public Guid? UsuarioId { get; set; }
        public string? NombreUsuario { get; set; }
        public string? Ip { get; set; }
        public string? UserAgent { get; set; }
        public string? Modulo { get; set; }
        public string? Observacion { get; set; }
        public DateTime Fecha { get; set; }
    }
}
