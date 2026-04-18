using System.ComponentModel.DataAnnotations;

namespace CapiMovil.PL.Gui.Models.Api
{
    public class CambiarEstadoIncidenciaRequestDto
    {
        [Required]
        public string EstadoIncidencia { get; set; } = string.Empty;
    }
}