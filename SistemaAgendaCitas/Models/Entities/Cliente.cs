using System.ComponentModel.DataAnnotations;

namespace SistemaAgendaCitas.Models.Entities
{
    public class Cliente
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Nombre es requerido.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 50 caracteres.")]
        public string Nombre { get; set; } = null!;

        [Required(ErrorMessage = "Apellido es requerido.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "El apellido debe tener entre 2 y 50 caracteres.")]
        public string Apellido { get; set; } = null!;

        [Required(ErrorMessage = "Email es requerido.")]
        [EmailAddress(ErrorMessage = "Formato de email inválido.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Teléfono es requerido.")]
        [Phone(ErrorMessage = "Formato de teléfono inválido.")]
        public string Telefono { get; set; } = null!;

        [Required(ErrorMessage = "Fecha de registro es requerida.")]
        public DateTime FechaRegistro { get; set; }

        // Relaciones
        public ICollection<Cita> Citas { get; set; } = new List<Cita>();
    }
}
