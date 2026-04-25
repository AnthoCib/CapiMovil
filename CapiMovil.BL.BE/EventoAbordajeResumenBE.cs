namespace CapiMovil.BL.BE
{
    public class EventoAbordajeResumenBE
    {
        public Guid IdRecorrido { get; set; }
        public Guid IdEstudiante { get; set; }
        public int TotalEventos { get; set; }
        public int TotalSubidas { get; set; }
        public int TotalBajadas { get; set; }
        public int TotalAusentes { get; set; }
        public int TotalNoAbordo { get; set; }
        public string? UltimoTipoEvento { get; set; }
    }
}
