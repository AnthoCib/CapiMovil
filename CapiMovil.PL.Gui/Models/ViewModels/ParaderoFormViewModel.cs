using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace CapiMovil.PL.Gui.Models.ViewModels
{
    public class ParaderoFormViewModel
    {
        public Guid IdParadero { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una ruta.")]
        [Display(Name = "Ruta")]
        public Guid IdRuta { get; set; }

        [Display(Name = "Código")]
        public string CodigoParadero { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La dirección es obligatoria.")]
        [Display(Name = "Dirección")]
        public string Direccion { get; set; } = string.Empty;

        [Display(Name = "Latitud")]
        public decimal? Latitud { get; set; }

        [Display(Name = "Longitud")]
        public decimal? Longitud { get; set; }

        [Required(ErrorMessage = "El orden de parada es obligatorio.")]
        [Range(1, 999, ErrorMessage = "El orden de parada debe ser mayor a 0.")]
        [Display(Name = "Orden de Parada")]
        public int OrdenParada { get; set; }

        [Display(Name = "Hora Estimada")]
        [DataType(DataType.Time)]
        public TimeSpan? HoraEstimada { get; set; }

        [Display(Name = "Estado")]
        public bool Estado { get; set; } = true;

        public List<SelectListItem> Rutas { get; set; } = new();
    }
}