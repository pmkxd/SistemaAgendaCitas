using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaAgendaCitas.Data;
using SistemaAgendaCitas.Models;
using SistemaAgendaCitas.Models.Entities;
using SistemaAgendaCitas.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaAgendaCitas.Controllers
{
    public class ServiciosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ServiciosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Servicios
        public async Task<IActionResult> Index(string estado)
        {
            ViewData["EstadoActual"] = estado;

            var servicios = _context.Servicios.AsQueryable();

            // Filtrar por estado si se especifica
            if (!string.IsNullOrEmpty(estado))
            {
                if (estado == "activos")
                    servicios = servicios.Where(s => s.Activo);
                else if (estado == "inactivos")
                    servicios = servicios.Where(s => !s.Activo);
            }

            return View(await servicios.ToListAsync());
        }

        // GET: Servicios/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var servicio = await _context.Servicios
                .FirstOrDefaultAsync(m => m.Id == id);
            if (servicio == null)
            {
                return NotFound();
            }

            return View(servicio);
        }

        // GET: Servicios/Create
        [HttpGet]
        public IActionResult Create()
        {
            var viewModel = new AddServicioViewModel
            {
                Activo = true // valor por defecto
            };
            return View(viewModel);
        }


        // POST: Servicios/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddServicioViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var servicio = new Servicio
                {
                    Nombre = viewModel.Nombre,
                    Descripcion = viewModel.Descripcion,
                    Duracion = viewModel.Duracion,
                    Precio = viewModel.Precio,
                    Activo = viewModel.Activo
                };

                _context.Servicios.Add(servicio);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(viewModel);
        }


        // GET: Servicios/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var servicio = await _context.Servicios.FindAsync(id);
            if (servicio == null) return NotFound();

            var viewModel = new AddServicioViewModel
            {
                Id = servicio.Id,
                Nombre = servicio.Nombre,
                Descripcion = servicio.Descripcion,
                Duracion = servicio.Duracion,
                Precio = servicio.Precio,
                Activo = servicio.Activo
            };

            return View(viewModel);
        }


        // POST: Servicios/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AddServicioViewModel viewModel)
        {
            if (id != viewModel.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var servicio = await _context.Servicios.FindAsync(id);
                if (servicio == null) return NotFound();

                servicio.Nombre = viewModel.Nombre;
                servicio.Descripcion = viewModel.Descripcion;
                servicio.Duracion = viewModel.Duracion;
                servicio.Precio = viewModel.Precio;
                servicio.Activo = viewModel.Activo;

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(viewModel);
        }


        // GET: Servicios/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var servicio = await _context.Servicios
                .FirstOrDefaultAsync(m => m.Id == id);
            if (servicio == null)
            {
                return NotFound();
            }

            return View(servicio);
        }

        // POST: Servicios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var servicio = await _context.Servicios.FindAsync(id);
            if (servicio == null) return NotFound();

            // Verificar si tiene citas pendientes
            bool tieneCitasPendientes = await _context.Citas
                .AnyAsync(c => c.ServicioId == id && c.Estado == EstadoCita.Pendiente);

            if (tieneCitasPendientes)
            {
                TempData["Error"] = "No se puede eliminar el servicio porque tiene citas pendientes asociadas.";
                return RedirectToAction(nameof(Index));
            }

            // Eliminación lógica: marcar como inactivo
            servicio.Activo = false;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool ServicioExists(int id)
        {
            return _context.Servicios.Any(e => e.Id == id);
        }
    }
}
