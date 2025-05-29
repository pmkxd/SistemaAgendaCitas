using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaAgendaCitas.Data;
using SistemaAgendaCitas.Models.Entities;
using SistemaAgendaCitas.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaAgendaCitas.Controllers
{
    public class CitasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CitasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Citas
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Citas.Include(c => c.Cliente).Include(c => c.Servicio);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Citas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cita = await _context.Citas
                .Include(c => c.Cliente)
                .Include(c => c.Servicio)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cita == null)
            {
                return NotFound();
            }

            return View(cita);
        }

        // GET: Citas/Create
        public IActionResult Create()
        {
            var viewModel = new AddCitaViewModel
            {

                Fecha = DateTime.Today,               
                Hora = new TimeSpan(0, 0, 0),        

                Clientes = _context.Clientes
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Nombre
                    }).ToList(),

                Servicios = _context.Servicios
                    .Select(s => new SelectListItem
                    {
                        Value = s.Id.ToString(),
                        Text = s.Nombre
                    }).ToList()
            };

            return View(viewModel);
        }


        // POST: Citas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddCitaViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var cita = new Cita
                {
                    ClienteId = viewModel.ClienteId,
                    ServicioId = viewModel.ServicioId,
                    Fecha = viewModel.Fecha,
                    Hora = viewModel.Hora,
                    Estado = viewModel.Estado,
                    Comentarios = viewModel.Comentarios
                };

                _context.Add(cita);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Si hay errores, recargar combos
            viewModel.Clientes = _context.Clientes
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Nombre
                }).ToList();

            viewModel.Servicios = _context.Servicios
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Nombre
                }).ToList();

            return View(viewModel);
        }


        // GET: Citas/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var cita = await _context.Citas.FindAsync(id);
            if (cita == null)
            {
                return NotFound();
            }

            var viewModel = new AddCitaViewModel
            {
                Id = cita.Id,
                ClienteId = cita.ClienteId,
                ServicioId = cita.ServicioId,
                Fecha = cita.Fecha,
                Hora = cita.Hora,
                Estado = cita.Estado,
                Comentarios = cita.Comentarios,

                Clientes = _context.Clientes
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Nombre
                    }).ToList(),

                Servicios = _context.Servicios
                    .Select(s => new SelectListItem
                    {
                        Value = s.Id.ToString(),
                        Text = s.Nombre
                    }).ToList()
            };

            return View(viewModel);
        }


        // POST: Citas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AddCitaViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var cita = await _context.Citas.FindAsync(id);
                if (cita == null)
                {
                    return NotFound();
                }

                cita.ClienteId = viewModel.ClienteId;
                cita.ServicioId = viewModel.ServicioId;
                cita.Fecha = viewModel.Fecha;
                cita.Hora = viewModel.Hora;
                cita.Estado = viewModel.Estado;
                cita.Comentarios = viewModel.Comentarios;

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Si falla validación, recargar combos
            viewModel.Clientes = _context.Clientes
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Apellido + ", " + c.Nombre
                }).ToList();

            viewModel.Servicios = _context.Servicios
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Nombre
                }).ToList();

            return View(viewModel);
        }


        // GET: Citas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cita = await _context.Citas
                .Include(c => c.Cliente)
                .Include(c => c.Servicio)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cita == null)
            {
                return NotFound();
            }

            return View(cita);
        }

        // POST: Citas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cita = await _context.Citas.FindAsync(id);
            if (cita != null)
            {
                _context.Citas.Remove(cita);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CitaExists(int id)
        {
            return _context.Citas.Any(e => e.Id == id);
        }
    }
}
