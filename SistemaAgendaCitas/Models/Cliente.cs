namespace SistemaAgendaCitas.Models
{
    public class Cliente
    {
        public int Id { get; set; } // Clave primaria
        public string Nombre { get; set; } = null!;
        public string Apellido { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Telefono { get; set; } = null!;
        public DateTime FechaRegistro { get; set; }

        // Relaciones
        public ICollection<Cita> Citas { get; set; } = new List<Cita>();
    }
}
