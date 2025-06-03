namespace SistemaAgendaCitas.Data.IRepositories;
using SistemaAgendaCitas.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IServicioRepository
{
    Task<IEnumerable<Servicio>> ObtenerTodosAsync();
    Task<Servicio> ObtenerPorIdAsync(int id);
    Task AgregarAsync(Servicio servicio);
    Task ActualizarAsync(Servicio servicio);
    Task EliminarAsync(int id);
    Task<bool> ExistePorIdAsync(int id);
    Task<List<Servicio>> ObtenerPorEstadoAsync(bool? activo);
}

