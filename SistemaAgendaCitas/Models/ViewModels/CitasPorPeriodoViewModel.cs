namespace SistemaAgendaCitas.Models.ViewModels
{
    public class CitasPorPeriodoViewModel
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public int Pendientes { get; set; }
        public int Confirmadas { get; set; }
        public int Completadas { get; set; }
        public int Canceladas { get; set; }
    }
}
