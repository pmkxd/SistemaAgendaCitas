using System.ComponentModel.DataAnnotations;

namespace SistemaAgendaCitas.Models.ViewModels
{
    public class CambiarEstadoCitaViewModel
    {
        public int Id { get; set; }

        public string ClienteNombre { get; set; } = "";

        public string ServicioNombre { get; set; } = "";

        public EstadoCita EstadoActual { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un nuevo estado.")]
        public EstadoCita NuevoEstado { get; set; }
    }
}
