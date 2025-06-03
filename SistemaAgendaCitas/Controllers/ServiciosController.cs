using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaAgendaCitas.Data;
using SistemaAgendaCitas.Data.IRepositories;
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
        private readonly IServicioRepository _servicioRepository;
        private readonly ILogger<ServiciosController> _logger;
        private readonly ICitaRepository _citaRepository;

        public ServiciosController(IServicioRepository servicioRepository, ILogger<ServiciosController> logger, ICitaRepository citaRepository)
        {
            _servicioRepository = servicioRepository;
            _logger = logger;
            _citaRepository = citaRepository;
        }

        // GET: Servicios
        public async Task<IActionResult> Index(string estado)
        {
            _logger.LogInformation("Accediendo a Index de Servicios con filtro de estado: {Estado}", estado);

            bool? estadoActivo = estado == "activos" ? true :
                     estado == "inactivos" ? false : (bool?)null;

            var servicios = await _servicioRepository.ObtenerPorEstadoAsync(estadoActivo);
            return View(servicios);

        }


        // GET: Servicios/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var servicio = await _servicioRepository.ObtenerPorIdAsync(id.Value);

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
            _logger.LogInformation("Intentando crear nuevo servicio: {Nombre}, Activo={Activo}", viewModel.Nombre, viewModel.Activo);

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

                await _servicioRepository.AgregarAsync(servicio);

                _logger.LogInformation("Servicio creado exitosamente: ID={Id}", servicio.Id);

                return RedirectToAction(nameof(Index));
            }

            _logger.LogWarning("Falló la validación al crear servicio. Errores: {Errores}",
                string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));

            return View(viewModel);
        }



        // GET: Servicios/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var servicio = await _servicioRepository.ObtenerPorIdAsync(id);
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
            if (id != viewModel.Id)
            {
                _logger.LogWarning("ID de ruta ({RutaId}) no coincide con ID del modelo ({ModeloId})", id, viewModel.Id);
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var servicio = await _servicioRepository.ObtenerPorIdAsync(id);
                if (servicio == null)
                {
                    _logger.LogWarning("Servicio con ID={Id} no encontrado para edición", id);
                    return NotFound();
                }

                servicio.Nombre = viewModel.Nombre;
                servicio.Descripcion = viewModel.Descripcion;
                servicio.Duracion = viewModel.Duracion;
                servicio.Precio = viewModel.Precio;
                servicio.Activo = viewModel.Activo;

                await _servicioRepository.ActualizarAsync(servicio);
                _logger.LogInformation("Servicio ID={Id} actualizado correctamente.", id);

                return RedirectToAction(nameof(Index));
            }

            _logger.LogWarning("ModelState inválido al editar servicio ID={Id}", viewModel.Id);
            return View(viewModel);
        }



        // GET: Servicios/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var servicio = await _servicioRepository.ObtenerPorIdAsync(id.Value);
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
            try
            {
                var servicio = await _servicioRepository.ObtenerPorIdAsync(id);
                if (servicio == null)
                {
                    _logger.LogWarning("Intento de eliminar servicio no existente. ID={Id}", id);
                    return NotFound();
                }

                bool tieneCitasPendientes = await _citaRepository.ExisteCitaPendientePorServicioAsync(id);

                if (tieneCitasPendientes)
                {
                    _logger.LogWarning("No se puede desactivar servicio ID={Id} porque tiene citas pendientes.", id);
                    TempData["Error"] = "No se puede eliminar el servicio porque tiene citas pendientes asociadas.";
                    return RedirectToAction(nameof(Index));
                }

                servicio.Activo = false;
                await _servicioRepository.ActualizarAsync(servicio);
                _logger.LogInformation("Servicio marcado como inactivo correctamente. ID={Id}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al intentar eliminar servicio ID={Id}", id);
                TempData["Error"] = "Ocurrió un error al eliminar el servicio.";
            }

            return RedirectToAction(nameof(Index));
        }


        private async Task<bool> ServicioExists(int id)
        {
            return await _servicioRepository.ExistePorIdAsync(id);
        }
    }
}
