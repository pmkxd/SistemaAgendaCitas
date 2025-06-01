using System.ComponentModel.DataAnnotations;

namespace SistemaAgendaCitas.Models.Entities
{
    public class Cita
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Cliente es requerido.")]
        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; } = null!;

        [Required(ErrorMessage = "Servicio es requerido.")]
        public int ServicioId { get; set; }
        public Servicio Servicio { get; set; } = null!;

        [Required(ErrorMessage = "Fecha es requerida.")]
        [DataType(DataType.Date)]
        public DateTime Fecha { get; set; }

        [Required(ErrorMessage = "Hora es requerida.")]
        [DataType(DataType.Time)]
        public TimeSpan Hora { get; set; }

        [Required(ErrorMessage = "Estado es requerido.")]
        public EstadoCita Estado { get; set; } = EstadoCita.Pendiente;
        public DateTime? FechaCambioEstado { get; set; }

        [StringLength(250, ErrorMessage = "Los comentarios no deben superar los 250 caracteres.")]
        public string? Comentarios { get; set; }

        

    }
}
