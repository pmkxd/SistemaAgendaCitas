using System.ComponentModel.DataAnnotations;

namespace SistemaAgendaCitas.Models.ViewModels
{
    public class AddServicioViewModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "El nombre del servicio es obligatorio.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres.")]
        public string Nombre { get; set; } = null!;

        [Required(ErrorMessage = "La descripción del servicio es obligatoria.")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "La descripción debe tener entre 1 y 100 caracteres.")]
        public string Descripcion { get; set; } = null!;

        [Required(ErrorMessage = "La duración del servicio es obligatoria.")]
        [Range(1, int.MaxValue, ErrorMessage = "La duración debe ser mayor a 0 minutos.")]
        public int Duracion { get; set; }

        [Required(ErrorMessage = "El precio del servicio es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a $0.")]
        public double Precio { get; set; }

        [Required(ErrorMessage = "Debe indicar si el servicio está activo.")]
        public bool Activo { get; set; }
    }
}
