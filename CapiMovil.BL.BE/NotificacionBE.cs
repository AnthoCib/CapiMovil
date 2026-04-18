namespace CapiMovil.BL.BE
{
    public class NotificacionBE
    {
        public Guid IdNotificacion { get; set; }
        public Guid IdPadre { get; set; }
        public Guid? IdEstudiante { get; set; }

        public string CodigoNotificacion { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public string TipoNotificacion { get; set; } = string.Empty;
        public string Canal { get; set; } = string.Empty;

        public bool Leido { get; set; }
        public DateTime? FechaLectura { get; set; }
        public DateTime FechaEnvio { get; set; }

        public bool Estado { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public DateTime? FechaEliminacion { get; set; }

        public string? NombrePadre { get; set; }
        public string? NombreEstudiante { get; set; }
    }
}