namespace SistemaAgendaCitas.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using SistemaAgendaCitas.Data;
using SistemaAgendaCitas.Data.IRepositories;
using SistemaAgendaCitas.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

public class ServicioRepository : IServicioRepository
{
    private readonly ApplicationDbContext _context;

    public ServicioRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Servicio>> ObtenerTodosAsync()
    {
        return await _context.Servicios.ToListAsync();
    }

    public async Task<Servicio> ObtenerPorIdAsync(int id)
    {
        return await _context.Servicios.FindAsync(id);
    }

    public async Task AgregarAsync(Servicio servicio)
    {
        _context.Servicios.Add(servicio);
        await _context.SaveChangesAsync();
    }

    public async Task ActualizarAsync(Servicio servicio)
    {
        _context.Servicios.Update(servicio);
        await _context.SaveChangesAsync();
    }

    public async Task EliminarAsync(int id)
    {
        var servicio = await _context.Servicios.FindAsync(id);
        if (servicio != null)
        {
            _context.Servicios.Remove(servicio);
            await _context.SaveChangesAsync();
        }
    }
    public async Task<bool> ExistePorIdAsync(int id)
    {
        return await _context.Servicios.AnyAsync(e => e.Id == id);

    }
    public async Task<List<Servicio>> ObtenerPorEstadoAsync(bool? activo)
    {
        var query = _context.Servicios.AsQueryable();
        if (activo.HasValue)
            query = query.Where(s => s.Activo == activo.Value);

        return await query.ToListAsync();
    }
}

