using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaAgendaCitas.Data;
using SistemaAgendaCitas.Data.IRepositories;
using SistemaAgendaCitas.Data.Repositories;
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
        private readonly ICitaRepository _citaRepository;
        private readonly ILogger<CitasController> _logger;
        private readonly IClienteRepository _clienteRepository;
        private readonly IServicioRepository _servicioRepository;

        public CitasController(ICitaRepository citaRepository, ILogger<CitasController> logger, IClienteRepository clienteRepository, IServicioRepository servicioRepository)
        {
            _citaRepository = citaRepository;
            _logger = logger;
            _clienteRepository = clienteRepository;
            _servicioRepository = servicioRepository;
        }

        // GET: Citas
        public async Task<IActionResult> Index(DateTime? fecha, int? clienteId, EstadoCita? estado)
        {
            _logger.LogInformation("Accediendo a Index de Citas. Filtros: Fecha={Fecha}, ClienteId={ClienteId}, Estado={Estado}",
                fecha, clienteId, estado);

            try
            {
                var clientes = await _clienteRepository.ObtenerTodosAsync();
                var citasQuery = _citaRepository.ObtenerCitasConClientesYServicios();

                if (fecha.HasValue)
                    citasQuery = citasQuery.Where(c => c.Fecha.Date == fecha.Value.Date);

                if (clienteId.HasValue)
                    citasQuery = citasQuery.Where(c => c.ClienteId == clienteId.Value);

                if (estado.HasValue)
                    citasQuery = citasQuery.Where(c => c.Estado == estado.Value);

                var listaCitas = await citasQuery.ToListAsync();

                var modelo = new IndexCitasViewModel
                {
                    Fecha = fecha,
                    ClienteId = clienteId,
                    Estado = estado,
                    Clientes = clientes
                        .Select(c => new SelectListItem
                        {
                            Value = c.Id.ToString(),
                            Text = c.Nombre,
                            Selected = clienteId.HasValue && c.Id == clienteId.Value
                        }).ToList(),
                    Estados = Enum.GetValues(typeof(EstadoCita))
                        .Cast<EstadoCita>()
                        .Select(e => new SelectListItem
                        {
                            Value = ((int)e).ToString(),
                            Text = e.ToString(),
                            Selected = estado.HasValue && (int)estado.Value == (int)e
                        }).ToList(),
                    Citas = listaCitas
                };

                return View(modelo);
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

            var cita = await _citaRepository.ObtenerPorIdAsync(id);
            if (cita == null)
            {
                return NotFound();
            }

            return View(cita);
        }

        // GET: Citas/Create
        public async Task<IActionResult> Create()
        {
            var clientes = await _clienteRepository.ObtenerTodosAsync();
            var servicios = await _servicioRepository.ObtenerTodosAsync();

            var viewModel = new AddCitaViewModel
            {
                Fecha = DateTime.Today,
                Hora = DateTime.Now.TimeOfDay,
                Clientes = clientes.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Nombre
                }).ToList(),
                Servicios = servicios.Select(s => new SelectListItem
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
            _logger.LogInformation("Intentando crear una nueva cita para ClienteId={ClienteId}, ServicioId={ServicioId}, Fecha={Fecha}, Hora={Hora}",
                viewModel.ClienteId, viewModel.ServicioId, viewModel.Fecha, viewModel.Hora);

            ModelState.Remove("Cliente");
            ModelState.Remove("Servicio");

            bool existeSolapamiento = await _citaRepository.ExisteCitaEnFechaHoraAsync(viewModel.Fecha, viewModel.Hora);

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

                await _citaRepository.AgregarAsync(cita);

                _logger.LogInformation("Cita creada exitosamente con ID={Id}", cita.Id);
                return RedirectToAction(nameof(Index));
            }

            _logger.LogWarning("ModelState inválido al intentar crear cita.");
            // recargar combos
            var clientes = await _clienteRepository.ObtenerTodosAsync();
            var servicios = await _servicioRepository.ObtenerTodosAsync();

            viewModel.Clientes = clientes
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Nombre
                }).ToList();

            viewModel.Servicios = servicios
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Nombre
                }).ToList();

            return View(viewModel);
        }




        // GET: Citas/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var cita = await _citaRepository.ObtenerPorIdAsync(id);

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
                var cita = await _citaRepository.BuscarPorIdAsync(id);
                if (cita == null)
                {
                    _logger.LogWarning("No se encontró la cita con ID={Id} para edición.", id);
                    return NotFound();
                }

                bool existeSolapamiento = await _citaRepository.ExisteSolapamientoAsync(viewModel.Id, viewModel.Fecha, viewModel.Hora);

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
                        bool puedeCancelar =
                            (cita.Estado == EstadoCita.Pendiente || cita.Estado == EstadoCita.Confirmada) &&
                            viewModel.Estado == EstadoCita.Cancelada;

                        bool puedeCompletar =
                            (cita.Estado == EstadoCita.Pendiente || cita.Estado == EstadoCita.Confirmada) &&
                            viewModel.Estado == EstadoCita.Completada;

                        bool puedeConfirmar =
                            cita.Estado == EstadoCita.Pendiente &&
                            viewModel.Estado == EstadoCita.Confirmada;

                        if (!puedeCancelar && !puedeCompletar && !puedeConfirmar)
                        {
                            _logger.LogWarning("Cambio de estado inválido en cita ID={Id}: de {Actual} a {Nuevo}", id, cita.Estado, viewModel.Estado);
                            ModelState.AddModelError("", "Solo puedes confirmar, cancelar o completar una cita según su estado actual.");
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

                        await _citaRepository.ActualizarAsync(cita);
                        _logger.LogInformation("Cita ID={Id} editada correctamente.", id);
                        return RedirectToAction(nameof(Index));
                    }
                }
            }

            _logger.LogWarning("ModelState inválido al editar cita ID={Id}", viewModel.Id);

            var citaCompleta = await _citaRepository.ObtenerPorIdAsync(viewModel.Id);

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

            var cita = await _citaRepository.ObtenerPorIdAsync(id);
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
                var cita = await _citaRepository.BuscarPorIdAsync(id);
                if (cita != null)
                {
                    await _citaRepository.EliminarAsync(id);
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


        private async Task<bool> CitaExists(int id)
        {
            return await _citaRepository.ExistePorIdAsync(id);
        }




        [HttpGet]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            var cita = await _citaRepository.ObtenerPorIdAsync(id);

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
        public async Task<IActionResult> CambiarEstadoRapido(int id, EstadoCita nuevoEstado)
        {
            var cita = await _citaRepository.BuscarPorIdAsync(id);
            if (cita == null)
            {
                TempData["Error"] = "No se encontró la cita.";
                return RedirectToAction(nameof(Index));
            }

            bool puedeCancelar =
                (cita.Estado == EstadoCita.Pendiente || cita.Estado == EstadoCita.Confirmada) &&
                nuevoEstado == EstadoCita.Cancelada;

            bool puedeCompletar =
                (cita.Estado == EstadoCita.Pendiente || cita.Estado == EstadoCita.Confirmada) &&
                nuevoEstado == EstadoCita.Completada;

            bool puedeConfirmar =
                cita.Estado == EstadoCita.Pendiente && nuevoEstado == EstadoCita.Confirmada;

            if (!puedeCancelar && !puedeCompletar && !puedeConfirmar)
            {
                TempData["Error"] = $"No se puede cambiar el estado desde {cita.Estado} a {nuevoEstado}.";
                return RedirectToAction(nameof(Index));
            }

            cita.Estado = nuevoEstado;
            cita.FechaCambioEstado = DateTime.Now;

            await _citaRepository.ActualizarAsync(cita);
            TempData["Success"] = $"Cita marcada como {nuevoEstado}.";
            return RedirectToAction(nameof(Index));
        }




        private async Task CargarDatosEstadoCita(CambiarEstadoCitaViewModel viewModel)
        {
            var citaOriginal = await _citaRepository.ObtenerPorIdAsync(viewModel.Id);

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
            var citas = _citaRepository.ObtenerCitasConClienteYServicio()
                .Select(c => new
                {
                    id = c.Id,
                    title = $"{c.Cliente.Nombre} - {c.Servicio.Nombre}",
                    start = c.Fecha.Add(c.Hora).ToString("yyyy-MM-ddTHH:mm:ss"),
                    allDay = false,
                    color = c.Estado == EstadoCita.Completada ? "#28a745" :
                            c.Estado == EstadoCita.Cancelada ? "#dc3545" :
                            c.Estado == EstadoCita.Confirmada ? "#ffc107" : "#0d6efd"
                })
                .ToList();

            return Json(citas);
        }


        [HttpGet]
        public async Task<IActionResult> Reportes(DateTime? fechaInicio, DateTime? fechaFin)

        {
            // Si no se envían fechas, usar último mes
            fechaInicio ??= DateTime.Today.AddMonths(-1);
            fechaFin ??= DateTime.Today;

            var citas = await _citaRepository.ObtenerConServicioPorRangoFechaAsync(fechaInicio.Value, fechaFin.Value);


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
