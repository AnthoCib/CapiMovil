namespace CapiMovil.BL.BE
{
    public class ParaderoBE
    {
        public Guid IdParadero { get; set; }
        public Guid IdRuta { get; set; }

        public string CodigoParadero { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public decimal? Latitud { get; set; }
        public decimal? Longitud { get; set; }
        public int OrdenParada { get; set; }
        public TimeSpan? HoraEstimada { get; set; }

        public bool Estado { get; set; } = true;

        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public DateTime? FechaEliminacion { get; set; }

        public RutaBE? Ruta { get; set; }
    }
}