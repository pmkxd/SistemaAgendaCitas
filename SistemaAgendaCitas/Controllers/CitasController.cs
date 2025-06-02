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
        private readonly ILogger<CitasController> _logger;

        public CitasController(ApplicationDbContext context, ILogger<CitasController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Citas
        public async Task<IActionResult> Index(DateTime? fecha, int? clienteId, EstadoCita? estado)
        {
            _logger.LogInformation("Accediendo a Index de Citas. Filtros: Fecha={Fecha}, ClienteId={ClienteId}, Estado={Estado}",
                fecha, clienteId, estado);

            try
            {
                var clientesLista = await _context.Clientes
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Apellido
                    }).ToListAsync();
                ViewBag.Clientes = new SelectList(clientesLista, "Value", "Text", clienteId?.ToString());

                ViewBag.Estados = Enum.GetValues(typeof(EstadoCita))
                    .Cast<EstadoCita>()
                    .Select(e => new SelectListItem
                    {
                        Value = ((int)e).ToString(),
                        Text = e.ToString(),
                        Selected = estado.HasValue && (int)e == (int)estado.Value
                    }).ToList();

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

                var lista = await citas.ToListAsync();
                _logger.LogInformation("Se cargaron {Cantidad} citas.", lista.Count);
                return View(lista);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Index de Citas.");
                return View("Error");
            }
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
            _logger.LogInformation("Intentando crear una nueva cita para ClienteId={ClienteId}, ServicioId={ServicioId}, Fecha={Fecha}, Hora={Hora}",
                viewModel.ClienteId, viewModel.ServicioId, viewModel.Fecha, viewModel.Hora);

            ModelState.Remove("Cliente");
            ModelState.Remove("Servicio");

            bool existeSolapamiento = await _context.Citas.AnyAsync(c =>
                c.Fecha == viewModel.Fecha && c.Hora == viewModel.Hora);

            if (existeSolapamiento)
            {
                _logger.LogWarning("Solapamiento detectado al crear cita en Fecha={Fecha}, Hora={Hora}", viewModel.Fecha, viewModel.Hora);
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

                _logger.LogInformation("Cita creada exitosamente con ID={Id}", cita.Id);
                return RedirectToAction(nameof(Index));
            }

            _logger.LogWarning("ModelState inválido al intentar crear cita.");
            // recargar combos
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
            if (id != viewModel.Id)
            {
                _logger.LogWarning("ID de ruta ({IdRuta}) no coincide con ID del modelo ({IdModelo})", id, viewModel.Id);
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var cita = await _context.Citas.FindAsync(id);
                if (cita == null)
                {
                    _logger.LogWarning("No se encontró la cita con ID={Id} para edición.", id);
                    return NotFound();
                }

                bool existeSolapamiento = await _context.Citas.AnyAsync(c =>
                    c.Id != viewModel.Id && c.Fecha == viewModel.Fecha && c.Hora == viewModel.Hora);

                if (existeSolapamiento)
                {
                    _logger.LogWarning("Solapamiento al editar cita ID={Id}", id);
                    ModelState.AddModelError(string.Empty, "Ya existe una cita registrada para esa fecha y hora.");
                }
                else
                {
                    bool cambioEstado = cita.Estado != viewModel.Estado;
                    if (cambioEstado)
                    {
                        bool puedeCancelar = cita.Estado == EstadoCita.Pendiente && viewModel.Estado == EstadoCita.Cancelada;
                        bool puedeCompletar = (cita.Estado == EstadoCita.Pendiente || cita.Estado == EstadoCita.Confirmada)
                            && viewModel.Estado == EstadoCita.Completada;

                        if (!puedeCancelar && !puedeCompletar)
                        {
                            _logger.LogWarning("Cambio de estado inválido en cita ID={Id}: de {Actual} a {Nuevo}", id, cita.Estado, viewModel.Estado);
                            ModelState.AddModelError("", "Solo puedes cancelar citas pendientes o completar citas pendientes/confirmadas.");
                        }
                        else
                        {
                            cita.Estado = viewModel.Estado;
                            cita.FechaCambioEstado = DateTime.Now;
                        }
                    }

                    if (ModelState.ErrorCount == 0)
                    {
                        cita.Fecha = viewModel.Fecha;
                        cita.Hora = viewModel.Hora;
                        cita.Comentarios = viewModel.Comentarios;

                        await _context.SaveChangesAsync();
                        _logger.LogInformation("Cita ID={Id} editada correctamente.", id);
                        return RedirectToAction(nameof(Index));
                    }
                }
            }

            _logger.LogWarning("ModelState inválido al editar cita ID={Id}", viewModel.Id);
            // recargar datos...
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
            try
            {
                var cita = await _context.Citas.FindAsync(id);
                if (cita != null)
                {
                    _context.Citas.Remove(cita);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Cita eliminada correctamente. ID={Id}", id);
                }
                else
                {
                    _logger.LogWarning("Intento de eliminar cita no existente. ID={Id}", id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al intentar eliminar cita ID={Id}", id);
                TempData["Error"] = "Ocurrió un error al eliminar la cita.";
            }

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
                _logger.LogWarning("ModelState inválido al cambiar estado de cita ID={Id}", viewModel.Id);
                await CargarDatosEstadoCita(viewModel);
                return View(viewModel);
            }

            var cita = await _context.Citas.FindAsync(viewModel.Id);
            if (cita == null)
            {
                _logger.LogWarning("No se encontró la cita con ID={Id} para cambiar estado", viewModel.Id);
                return NotFound();
            }

            bool puedeCancelar = cita.Estado == EstadoCita.Pendiente && viewModel.NuevoEstado == EstadoCita.Cancelada;
            bool puedeCompletar = (cita.Estado == EstadoCita.Pendiente || cita.Estado == EstadoCita.Confirmada)
                && viewModel.NuevoEstado == EstadoCita.Completada;

            if (!puedeCancelar && !puedeCompletar)
            {
                _logger.LogWarning("Intento de cambio de estado inválido. ID={Id}, EstadoActual={Actual}, NuevoEstado={Nuevo}",
                    viewModel.Id, cita.Estado, viewModel.NuevoEstado);

                ModelState.AddModelError("", "Solo puedes cancelar citas pendientes o completar citas pendientes/confirmadas.");
                await CargarDatosEstadoCita(viewModel);
                return View(viewModel);
            }

            cita.Estado = viewModel.NuevoEstado;
            cita.FechaCambioEstado = DateTime.Now;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Cita ID={Id} cambio de estado exitoso a {NuevoEstado}", viewModel.Id, viewModel.NuevoEstado);
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




        public IActionResult Calendario()
        {
            return View();
        }
        [HttpGet]
        public JsonResult ObtenerCitas()
        {
            var citas = _context.Citas
                .Include(c => c.Cliente)
                .Include(c => c.Servicio)


                .Select(c => new {
                    id = c.Id,
                    title = c.Cliente.Nombre + " - " + c.Servicio.Nombre,
                    start = c.Fecha.Add(c.Hora).ToString("s"), // formato ISO 8601
                    allDay = false
                })
                .ToList();


            return Json(citas);
        }

        [HttpGet]
        public IActionResult Reportes(DateTime? fechaInicio, DateTime? fechaFin)
        {
            // Si no se envían fechas, usar último mes
            fechaInicio ??= DateTime.Today.AddMonths(-1);
            fechaFin ??= DateTime.Today;

            var citas = _context.Citas
                .Include(c => c.Servicio)
                .Where(c => c.Fecha >= fechaInicio && c.Fecha <= fechaFin)
                .ToList();

            var modelo = new ReporteCitasYServiciosViewModel
            {
                FechaInicio = fechaInicio.Value,
                FechaFin = fechaFin.Value,
                Pendientes = citas.Count(c => c.Estado == EstadoCita.Pendiente),
                Confirmadas = citas.Count(c => c.Estado == EstadoCita.Confirmada),
                Completadas = citas.Count(c => c.Estado == EstadoCita.Completada),
                Canceladas = citas.Count(c => c.Estado == EstadoCita.Cancelada),
                EstadisticasServicios = citas
                    .Where(c => c.Estado != EstadoCita.Cancelada)
                    .GroupBy(c => c.Servicio.Nombre)
                    .Select(g => new ServicioEstadistica
                    {
                        NombreServicio = g.Key,
                        CantidadCitas = g.Count(),
                        IngresoTotal = g.Sum(c => c.Servicio.Precio)
                    })
                    .OrderByDescending(e => e.CantidadCitas)
                    .ToList()
            };

            return View(modelo);
        }


    }
}
