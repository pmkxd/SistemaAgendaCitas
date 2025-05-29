using System.ComponentModel.DataAnnotations;

namespace SistemaAgendaCitas.Models.ViewModels
{
    public class AddServicioViewModel
    {
        public int? Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Nombre { get; set; } = null!;

        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Descripcion { get; set; } = null!;

        [Required]
        [Range(1, int.MaxValue)]
        public int Duracion { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public double Precio { get; set; }

        [Required]
        public bool Activo { get; set; }
    }
}
