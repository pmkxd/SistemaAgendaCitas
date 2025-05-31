using System.ComponentModel.DataAnnotations;

namespace SistemaAgendaCitas.Models.ViewModels
{
    public class EditCitaViewModel
    {
        public int Id { get; set; }


        [Required]
        [DataType(DataType.Date)]
        public DateTime Fecha { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan Hora { get; set; }

        [Required]
        public EstadoCita Estado { get; set; }

        public string Comentarios { get; set; } = "";

        public string ClienteNombre { get; set; } = "";

        public string ServicioNombre { get; set; } = "";

    }

}
