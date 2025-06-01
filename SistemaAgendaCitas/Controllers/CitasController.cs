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
    public class CitasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CitasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Citas
        public async Task<IActionResult> Index(DateTime? fecha, int? clienteId, EstadoCita? estado)
        {
            var clientesLista = await _context.Clientes
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Apellido
                }).ToListAsync();

            var estadosLista = Enum.GetValues(typeof(EstadoCita))
                .Cast<EstadoCita>()
                .Select(e => new SelectListItem
                {
                    Value = ((int)e).ToString(),
                    Text = e.ToString()
                }).ToList();

            // Establecer selección activa
            ViewBag.Clientes = new SelectList(clientesLista, "Value", "Text", clienteId?.ToString());
            ViewBag.Estados = new SelectList(estadosLista, "Value", "Text", estado?.ToString());

            var citas = _context.Citas
                .Include(c => c.Cliente)
                .Include(c => c.Servicio)
                .AsQueryable();

            if (fecha.HasValue)
                citas = citas.Where(c => c.Fecha.Date == fecha.Value.Date);

            if (clienteId.HasValue)
                citas = citas.Where(c => c.ClienteId == clienteId.Value);

            if (estado.HasValue)
                citas = citas.Where(c => c.Estado == estado.Value);

            return View(await citas.ToListAsync());
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

            ViewData["ClienteId"] = new SelectList(_context.Clientes, "Id", "Nombre");
            ViewData["ServicioId"] = new SelectList(_context.Servicios, "Id", "Nombre");
            return View();

        }


        // POST: Citas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddCitaViewModel viewModel)
        {
            // Excluir propiedades de navegación si existen
            ModelState.Remove("Cliente");
            ModelState.Remove("Servicio");

            // Validar disponibilidad de horario (solapamiento)
            bool existeSolapamiento = await _context.Citas.AnyAsync(c =>
    c.Fecha == viewModel.Fecha &&
    c.Hora == viewModel.Hora);


            if (existeSolapamiento)
            {
                ModelState.AddModelError(string.Empty, "Ya existe una cita registrada para el mismo servicio, fecha y hora.");
            }

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

            // Si hay errores, recargar listas desplegables
            viewModel.Clientes = await _context.Clientes
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Nombre
                }).ToListAsync();

            viewModel.Servicios = await _context.Servicios
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Nombre
                }).ToListAsync();

            return View(viewModel);
        }



        // GET: Citas/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var cita = await _context.Citas
    .Include(c => c.Cliente)
    .Include(c => c.Servicio)
    .FirstOrDefaultAsync(c => c.Id == id);

            if (cita == null) return NotFound();

            var viewModel = new EditCitaViewModel
            {
                Id = cita.Id,
                Fecha = cita.Fecha,
                Hora = cita.Hora,
                Estado = cita.Estado,
                Comentarios = cita.Comentarios,
                ClienteNombre = cita.Cliente.Nombre,
                ServicioNombre = cita.Servicio.Nombre
            };

            return View(viewModel);
        }



        // POST: Citas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditCitaViewModel viewModel)
        {
            if (id != viewModel.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var cita = await _context.Citas.FindAsync(id);
                if (cita == null) return NotFound();

                // Validar solapamiento de fecha y hora
                bool existeSolapamiento = await _context.Citas.AnyAsync(c =>
                    c.Id != viewModel.Id &&
                    c.Fecha == viewModel.Fecha &&
                    c.Hora == viewModel.Hora);

                if (existeSolapamiento)
                {
                    ModelState.AddModelError(string.Empty, "Ya existe una cita registrada para esa fecha y hora.");
                }
                else
                {
                    // Validar si se intenta cambiar de estado
                    bool cambioEstado = cita.Estado != viewModel.Estado;
                    if (cambioEstado)
                    {
                        bool puedeCancelar = cita.Estado == EstadoCita.Pendiente && viewModel.Estado == EstadoCita.Cancelada;
                        bool puedeCompletar = (cita.Estado == EstadoCita.Pendiente || cita.Estado == EstadoCita.Confirmada)
                            && viewModel.Estado == EstadoCita.Completada;

                        if (!puedeCancelar && !puedeCompletar)
                        {
                            ModelState.AddModelError("", "Solo puedes cancelar citas pendientes o completar citas pendientes/confirmadas.");
                        }
                        else
                        {
                            cita.Estado = viewModel.Estado;
                            cita.FechaCambioEstado = DateTime.Now;
                        }
                    }

                    // Si no hay errores de validación extra
                    if (ModelState.ErrorCount == 0)
                    {
                        cita.Fecha = viewModel.Fecha;
                        cita.Hora = viewModel.Hora;
                        cita.Comentarios = viewModel.Comentarios;

                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                }
            }

            // Restaurar valores visibles si hubo errores
            var citaCompleta = await _context.Citas
                .Include(c => c.Cliente)
                .Include(c => c.Servicio)
                .FirstOrDefaultAsync(c => c.Id == viewModel.Id);

            if (citaCompleta != null)
            {
                viewModel.ClienteNombre = $"{citaCompleta.Cliente.Nombre} ";
                viewModel.ServicioNombre = citaCompleta.Servicio.Nombre;
            }

            viewModel.Comentarios ??= "";

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

        [HttpGet]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            var cita = await _context.Citas
                .Include(c => c.Cliente)
                .Include(c => c.Servicio)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cita == null) return NotFound();

            var viewModel = new CambiarEstadoCitaViewModel
            {
                Id = cita.Id,
                ClienteNombre = cita.Cliente.Nombre,
                ServicioNombre = cita.Servicio.Nombre,
                EstadoActual = cita.Estado
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(CambiarEstadoCitaViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                await CargarDatosEstadoCita(viewModel);
                return View(viewModel);
            }

            var cita = await _context.Citas.FindAsync(viewModel.Id);
            if (cita == null) return NotFound();

            // Validar estados permitidos para cambio
            bool puedeCancelar = cita.Estado == EstadoCita.Pendiente && viewModel.NuevoEstado == EstadoCita.Cancelada;
            bool puedeCompletar = (cita.Estado == EstadoCita.Pendiente || cita.Estado == EstadoCita.Confirmada) && viewModel.NuevoEstado == EstadoCita.Completada;

            if (!puedeCancelar && !puedeCompletar)
            {
                ModelState.AddModelError("", "Solo puedes cancelar citas pendientes o completar citas pendientes/confirmadas.");
                await CargarDatosEstadoCita(viewModel);
                return View(viewModel);
            }

            cita.Estado = viewModel.NuevoEstado;
            cita.FechaCambioEstado = DateTime.Now;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private async Task CargarDatosEstadoCita(CambiarEstadoCitaViewModel viewModel)
        {
            var citaOriginal = await _context.Citas
                .Include(c => c.Cliente)
                .Include(c => c.Servicio)
                .FirstOrDefaultAsync(c => c.Id == viewModel.Id);

            if (citaOriginal != null)
            {
                viewModel.ClienteNombre = citaOriginal.Cliente.Nombre + " " + citaOriginal.Cliente.Apellido;
                viewModel.ServicioNombre = citaOriginal.Servicio.Nombre;
                viewModel.EstadoActual = citaOriginal.Estado;
            }
        }



    }
}
