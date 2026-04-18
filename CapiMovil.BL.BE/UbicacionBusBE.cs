namespace CapiMovil.BL.BE
{
    public class UbicacionBusBE
    {
        public Guid IdUbicacion { get; set; }
        public Guid IdRecorrido { get; set; }

        public string CodigoUbicacion { get; set; } = string.Empty;
        public decimal Latitud { get; set; }
        public decimal Longitud { get; set; }
        public decimal? Velocidad { get; set; }
        public decimal? PrecisionMetros { get; set; }
        public DateTime FechaHora { get; set; }
        public string? Fuente { get; set; }

        public bool Estado { get; set; } = true;
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public DateTime? FechaEliminacion { get; set; }

        public RecorridoBE? Recorrido { get; set; }
    }
}