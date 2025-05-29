using System.ComponentModel.DataAnnotations;

namespace SistemaAgendaCitas.Models.ViewModels
{
    public class AddClienteViewModel
    {
        public int? Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Nombre { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string Apellido { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [Phone]
        public string Telefono { get; set; } = null!;

        [Required]
        public DateTime FechaRegistro { get; set; }
    }
}
