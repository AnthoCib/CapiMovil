namespace CapiMovil.BL.BE
{
    public class CalificacionConductorBE
    {
        public Guid IdCalificacion { get; set; }
        public Guid IdPadre { get; set; }
        public Guid IdConductor { get; set; }
        public Guid? IdEstudiante { get; set; }
        public int Puntaje { get; set; }
        public string? Comentario { get; set; }
        public DateTime FechaRegistro { get; set; }
        public bool Estado { get; set; } = true;

        public string? NombreConductor { get; set; }
    }
}
