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
using X.PagedList;
using X.PagedList.Extensions;


namespace SistemaAgendaCitas.Controllers
{
    public class ClientesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClientesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Clientes
        public async Task<IActionResult> Index(string orden, int? pagina)
        {
            var clientes = _context.Clientes.AsQueryable();

            ViewData["OrdenActual"] = orden;
            ViewData["OrdenPorNombre"] = string.IsNullOrEmpty(orden) ? "nombre_desc" : "";
            ViewData["OrdenPorFecha"] = orden == "fecha" ? "fecha_desc" : "fecha";

            // Ordenamiento
            clientes = orden switch
            {
                "nombre_desc" => clientes.OrderByDescending(c => c.Nombre.ToLower()),
                "fecha" => clientes.OrderBy(c => c.FechaRegistro),
                "fecha_desc" => clientes.OrderByDescending(c => c.FechaRegistro),
                _ => clientes.OrderBy(c => c.Nombre.ToLower())
            };

            // Paginación
            int tamanioPagina = 10;
            int numeroPagina = pagina ?? 1;

            return View(clientes.ToPagedList(numeroPagina, tamanioPagina));
        }

        // GET: Clientes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cliente == null)
            {
                return NotFound();
            }

            return View(cliente);
        }

        // GET: Clientes/Create
        [HttpGet]
        public IActionResult Create()
        {
            var viewModel = new AddClienteViewModel
            {
                FechaRegistro = DateTime.Today
            };
            return View(viewModel);
        }

        // POST: Clientes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddClienteViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                // Validar que no exista otro cliente con el mismo email
                bool emailExiste = await _context.Clientes.AnyAsync(c => c.Email == viewModel.Email);
                if (emailExiste)
                {
                    ModelState.AddModelError("Email", "El email ya está registrado.");
                }
                else
                {
                    var cliente = new Cliente
                    {
                        Nombre = viewModel.Nombre,
                        Apellido = viewModel.Apellido,
                        Email = viewModel.Email,
                        Telefono = viewModel.Telefono,
                        FechaRegistro = viewModel.FechaRegistro
                    };

                    _context.Clientes.Add(cliente);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }

            return View(viewModel);
        }



        // GET: Clientes/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null) return NotFound();

            var viewModel = new AddClienteViewModel
            {
                Id = cliente.Id,
                Nombre = cliente.Nombre,
                Apellido = cliente.Apellido,
                Email = cliente.Email,
                Telefono = cliente.Telefono,
                FechaRegistro = cliente.FechaRegistro
            };

            return View(viewModel);
        }


        // POST: Clientes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AddClienteViewModel viewModel)
        {
            if (id != viewModel.Id) return NotFound();

            if (ModelState.IsValid)
            {
                // Validar que el nuevo email no esté registrado por otro cliente
                bool emailExiste = await _context.Clientes
                    .AnyAsync(c => c.Email == viewModel.Email && c.Id != id);

                if (emailExiste)
                {
                    ModelState.AddModelError("Email", "El email ya está registrado por otro cliente.");
                }
                else
                {
                    var cliente = await _context.Clientes.FindAsync(id);
                    if (cliente == null) return NotFound();

                    cliente.Nombre = viewModel.Nombre;
                    cliente.Apellido = viewModel.Apellido;
                    cliente.Email = viewModel.Email;
                    cliente.Telefono = viewModel.Telefono;
                    cliente.FechaRegistro = viewModel.FechaRegistro;

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }

            return View(viewModel);
        }


        // GET: Clientes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cliente == null)
            {
                return NotFound();
            }

            return View(cliente);
        }

        // POST: Clientes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null) return NotFound();

            // Verificar si el cliente tiene citas asociadas
            bool tieneCitas = await _context.Citas.AnyAsync(c => c.ClienteId == id);

            if (tieneCitas)
            {
                TempData["Error"] = "No se puede eliminar el cliente porque tiene citas asociadas.";
                return RedirectToAction(nameof(Index));
            }

            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ClienteExists(int id)
        {
            return _context.Clientes.Any(e => e.Id == id);
        }
    }
}
