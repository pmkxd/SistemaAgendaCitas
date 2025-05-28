namespace SistemaAgendaCitas.Models
{
    public class Cita
    {
        public int Id { get; set; }

        // Claves foráneas
        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; } = null!;

        public int ServicioId { get; set; }
        public Servicio Servicio { get; set; } = null!;

        public DateTime Fecha { get; set; }
        public TimeSpan Hora { get; set; }

        // Estado de la cita (Pendiente, Confirmada, Completada, Cancelada)
        public string Estado { get; set; } = "Pendiente";

        public string? Comentarios { get; set; }
    }
}
