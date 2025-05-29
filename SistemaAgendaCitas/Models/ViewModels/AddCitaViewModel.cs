using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SistemaAgendaCitas.Models.ViewModels
{
    public class AddCitaViewModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Cliente es requerido.")]
        public int ClienteId { get; set; }

        [Required(ErrorMessage = "Servicio es requerido.")]
        public int ServicioId { get; set; }

        [Required(ErrorMessage = "Fecha es requerida.")]
        [DataType(DataType.Date)]
        public DateTime Fecha { get; set; }

        [Required(ErrorMessage = "Hora es requerida.")]
        [DataType(DataType.Time)]
        public TimeSpan Hora { get; set; }

        [Required(ErrorMessage = "Estado es requerido.")]
        public EstadoCita Estado { get; set; } = EstadoCita.Pendiente;

        [StringLength(250)]
        public string? Comentarios { get; set; }

        // Para los combos
        public List<SelectListItem> Clientes { get; set; } = new();
        public List<SelectListItem> Servicios { get; set; } = new();
    
}
}
