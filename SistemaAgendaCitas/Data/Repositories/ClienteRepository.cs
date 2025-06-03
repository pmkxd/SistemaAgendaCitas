namespace SistemaAgendaCitas.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using SistemaAgendaCitas.Data;
using SistemaAgendaCitas.Data.IRepositories;
using SistemaAgendaCitas.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

public class ClienteRepository : IClienteRepository
{
    private readonly ApplicationDbContext _context;

    public ClienteRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Cliente>> ObtenerTodosAsync()
    {
        return await _context.Clientes.ToListAsync();
    }

    public async Task<Cliente> ObtenerPorIdAsync(int id)
    {
        return await _context.Clientes.FindAsync(id);
    }

    public async Task AgregarAsync(Cliente cliente)
    {
        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();
    }

    public async Task ActualizarAsync(Cliente cliente)
    {
        _context.Clientes.Update(cliente);
        await _context.SaveChangesAsync();
    }

    public async Task EliminarAsync(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente != null)
        {
            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();
        }
    }
    public IQueryable<Cliente> ObtenerQueryable()
    {
        return _context.Clientes.AsQueryable();
    }
    public async Task<Cliente> BuscarPorIdAsync(int id)
    {
        return await _context.Clientes.FirstOrDefaultAsync(c => c.Id == id);
    }
    public async Task<bool> ExisteEmailAsync(string email)
    {
        return await _context.Clientes.AnyAsync(c => c.Email == email);
    }
    public async Task<bool> ExistePorIdAsync(int id)
    {
        return await _context.Clientes.AnyAsync(e => e.Id == id);
    }
    public async Task<bool> ExisteEmailEnOtroClienteAsync(string email, int? idClienteActual)
    {
        return await _context.Clientes
            .AnyAsync(c => c.Email == email && c.Id != idClienteActual);
    }
}