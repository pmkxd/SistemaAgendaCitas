namespace SistemaAgendaCitas.Data.IRepositories;
using SistemaAgendaCitas.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface ICitaRepository
{
    Task<IEnumerable<Cita>> ObtenerTodasAsync();
    Task<Cita> ObtenerPorIdAsync(int? id);
    Task AgregarAsync(Cita cita);
    Task ActualizarAsync(Cita cita);
    Task EliminarAsync(int id);

    // 🔍 Nuevo método
    Task<bool> ExistenCitasPorClienteAsync(int clienteId);
    IQueryable<Cita> ObtenerCitasConClientesYServicios();
    Task<bool> ExisteCitaEnFechaHoraAsync(DateTime fecha, TimeSpan hora);
    Task<Cita> BuscarPorIdAsync(int id);
    Task<bool> ExisteSolapamientoAsync(int idActual, DateTime fecha, TimeSpan hora);
    Task<bool> ExistePorIdAsync(int id);
    Task<List<Cita>> ObtenerConServicioPorRangoFechaAsync(DateTime fechaInicio, DateTime fechaFin);
    Task<bool> ExisteCitaPendientePorServicioAsync(int servicioId);
    IQueryable<Cita> ObtenerCitasConClienteYServicio();
}