using System.ComponentModel.DataAnnotations;

namespace CapiMovil.PL.Gui.Models.Api
{
    public class AuditoriaRequestDto
    {
        [Required]
        [StringLength(100)]
        public string Tabla { get; set; } = string.Empty;

        public Guid? IdRegistro { get; set; }

        [Required]
        [StringLength(20)]
        public string Accion { get; set; } = string.Empty;

        public string? DatosAntes { get; set; }
        public string? DatosDespues { get; set; }
        public Guid? UsuarioId { get; set; }
        public string? NombreUsuario { get; set; }
        public string? Ip { get; set; }
        public string? UserAgent { get; set; }
        public string? Modulo { get; set; }
        public string? Observacion { get; set; }
    }
}