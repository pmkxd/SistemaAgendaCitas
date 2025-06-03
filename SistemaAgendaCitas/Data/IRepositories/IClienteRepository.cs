namespace SistemaAgendaCitas.Data.IRepositories;
using SistemaAgendaCitas.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IClienteRepository
{
    Task<IEnumerable<Cliente>> ObtenerTodosAsync();
    Task<Cliente> ObtenerPorIdAsync(int id);
    Task AgregarAsync(Cliente cliente);
    Task ActualizarAsync(Cliente cliente);
    Task EliminarAsync(int id);
    IQueryable<Cliente> ObtenerQueryable(); // Devuelve IQueryable para ordenamientos/paginación
    Task<Cliente> BuscarPorIdAsync(int id); // Específico con FirstOrDefaultAsync
    Task<bool> ExisteEmailAsync(string email);
    Task<bool> ExistePorIdAsync(int id);
    Task<bool> ExisteEmailEnOtroClienteAsync(string email, int? idClienteActual);

}
