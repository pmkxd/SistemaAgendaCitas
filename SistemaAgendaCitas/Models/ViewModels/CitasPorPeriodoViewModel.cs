namespace SistemaAgendaCitas.Models.ViewModels
{
    public class ServicioEstadistica
    {
        public string NombreServicio { get; set; } = string.Empty;
        public int CantidadCitas { get; set; }
        public double IngresoTotal { get; set; }
    }

    public class ReporteCitasYServiciosViewModel
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }

        // Citas por estado
        public int Pendientes { get; set; }
        public int Confirmadas { get; set; }
        public int Completadas { get; set; }
        public int Canceladas { get; set; }

        // Servicios
        public List<ServicioEstadistica> EstadisticasServicios { get; set; } = new();
    }
}
