using System.ComponentModel.DataAnnotations;

namespace CapiMovil.PL.Gui.Models.Api
{
    public class CerrarIncidenciaRequestDto
    {
        [Required]
        [StringLength(300)]
        public string Solucion { get; set; } = string.Empty;
    }
}