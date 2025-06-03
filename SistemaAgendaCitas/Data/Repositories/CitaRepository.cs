namespace SistemaAgendaCitas.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using SistemaAgendaCitas.Data;
using SistemaAgendaCitas.Data.IRepositories;
using SistemaAgendaCitas.Models;
using SistemaAgendaCitas.Models.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class CitaRepository : ICitaRepository
{
    private readonly ApplicationDbContext _context;

    public CitaRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Cita>> ObtenerTodasAsync()
    {
        return await _context.Citas
            .Include(c => c.Cliente)
            .Include(c => c.Servicio)
            .ToListAsync();
    }

    public async Task<Cita> ObtenerPorIdAsync(int? id)
    {
        return await _context.Citas
            .Include(c => c.Cliente)
            .Include(c => c.Servicio)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task AgregarAsync(Cita cita)
    {
        _context.Citas.Add(cita);
        await _context.SaveChangesAsync();
    }

    public async Task ActualizarAsync(Cita cita)
    {
        _context.Citas.Update(cita);
        await _context.SaveChangesAsync();
    }

    public async Task EliminarAsync(int id)
    {
        var cita = await _context.Citas.FindAsync(id);
        if (cita != null)
        {
            _context.Citas.Remove(cita);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistenCitasPorClienteAsync(int clienteId)
    {
        return await _context.Citas.AnyAsync(c => c.ClienteId == clienteId);
    }
    public IQueryable<Cita> ObtenerCitasConClientesYServicios()
    {
        return _context.Citas
            .Include(c => c.Cliente)
            .Include(c => c.Servicio)
            .AsQueryable();
    }
    public async Task<bool> ExisteCitaEnFechaHoraAsync(DateTime fecha, TimeSpan hora)
    {
        return await _context.Citas.AnyAsync(c => c.Fecha == fecha && c.Hora == hora);
    }
    public async Task<Cita> BuscarPorIdAsync(int id)
    {
        return await _context.Citas.FindAsync(id);
    }

    public async Task<bool> ExisteSolapamientoAsync(int idActual, DateTime fecha, TimeSpan hora)
    {
        return await _context.Citas.AnyAsync(c =>
            c.Id != idActual &&
            c.Fecha == fecha &&
            c.Hora == hora);
    }
    public async Task<bool> ExistePorIdAsync(int id)
    {
        return await _context.Citas.AnyAsync(e => e.Id == id);
    }
    public async Task<List<Cita>> ObtenerConServicioPorRangoFechaAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        return await _context.Citas
            .Include(c => c.Servicio)
            .Where(c => c.Fecha >= fechaInicio && c.Fecha <= fechaFin)
            .ToListAsync();
    }
    public async Task<bool> ExisteCitaPendientePorServicioAsync(int servicioId)
    {
        return await _context.Citas
            .AnyAsync(c => c.ServicioId == servicioId && c.Estado == EstadoCita.Pendiente);
    }
}
