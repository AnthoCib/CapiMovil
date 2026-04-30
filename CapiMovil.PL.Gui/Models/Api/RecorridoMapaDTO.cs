namespace CapiMovil.PL.Gui.Models.Api
{
    public class RecorridoMapaDTO
    {
        public Guid IdRecorrido { get; set; }
        public string CodigoRecorrido { get; set; } = string.Empty;
        public string EstadoRecorrido { get; set; } = string.Empty;
        public RutaMapaDTO? Ruta { get; set; }
        public BusResumenDTO? Bus { get; set; }
        public UbicacionBusDTO? UbicacionActual { get; set; }
        public List<ParaderoMapaDTO> Paraderos { get; set; } = new();
    }

    public class RutaMapaDTO
    {
        public Guid IdRuta { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string PuntoInicio { get; set; } = string.Empty;
        public string PuntoFin { get; set; } = string.Empty;
        public decimal? LatitudInicio { get; set; }
        public decimal? LongitudInicio { get; set; }
        public decimal? LatitudFin { get; set; }
        public decimal? LongitudFin { get; set; }
    }

    public class ParaderoMapaDTO
    {
        public Guid IdParadero { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public int OrdenParada { get; set; }
        public string HoraEstimada { get; set; } = string.Empty;
        public decimal? Latitud { get; set; }
        public decimal? Longitud { get; set; }
        public int TotalAlumnos { get; set; }
    }

    public class UbicacionBusDTO
    {
        public decimal? Latitud { get; set; }
        public decimal? Longitud { get; set; }
        public DateTime? FechaHora { get; set; }
    }

    public class BusResumenDTO
    {
        public Guid IdBus { get; set; }
        public string CodigoBus { get; set; } = string.Empty;
        public string Placa { get; set; } = string.Empty;
    }
}
