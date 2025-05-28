using System.ComponentModel.DataAnnotations;

namespace SistemaAgendaCitas.Models.Entities
{
    public class Servicio
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = " Nombre es requerido.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = " El nombre del servicio debe ser entre 3 y 100 caracteres.")]
        public string Nombre { get; set; }
        [Required(ErrorMessage = " Descripcion es requerida.")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = " La descripcion del servicio debe ser entre 1 y 100 caracteres.")]
        public string Descripcion { get; set; }
        [Required(ErrorMessage = " Duracion es requerida.")]
        [Range(1, int.MaxValue, ErrorMessage = "La duración debe ser mayor a 0.")]
        public int Duracion { get; set; }
        [Required(ErrorMessage = " Precio es requerido.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0.")]
        public double Precio { get; set; }
        [Required(ErrorMessage = " Activo es requerido.")]
        public bool Activo { get; set; }
    }
}
