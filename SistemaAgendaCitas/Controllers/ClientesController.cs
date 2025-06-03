using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaAgendaCitas.Data;
using SistemaAgendaCitas.Data.IRepositories;
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
        private readonly IClienteRepository _clienteRepo;
        private readonly ILogger<ClientesController> _logger;
        private readonly ICitaRepository _citaRepo;

        public ClientesController(IClienteRepository clienteRepo, ILogger<ClientesController> logger, ICitaRepository citaRepo)
        {
            _clienteRepo = clienteRepo;
            _logger = logger;
            _citaRepo = citaRepo;
        }

        // GET: Clientes
        public IActionResult Index(string orden, int? pagina)
        {
            _logger.LogInformation("Accediendo a Index de Clientes con orden: {Orden}, Página: {Pagina}", orden, pagina);

            var clientes = _clienteRepo.ObtenerQueryable();

            ViewData["OrdenActual"] = orden;
            ViewData["OrdenPorNombre"] = string.IsNullOrEmpty(orden) ? "nombre_desc" : "";
            ViewData["OrdenPorFecha"] = orden == "fecha" ? "fecha_desc" : "fecha";

            clientes = orden switch
            {
                "nombre_desc" => clientes.OrderByDescending(c => c.Nombre.ToLower()),
                "fecha" => clientes.OrderBy(c => c.FechaRegistro),
                "fecha_desc" => clientes.OrderByDescending(c => c.FechaRegistro),
                _ => clientes.OrderBy(c => c.Nombre.ToLower())
            };

            int tamanioPagina = 10;
            int numeroPagina = pagina ?? 1;

            var paginados = clientes.ToPagedList(numeroPagina, tamanioPagina);

            _logger.LogInformation("Mostrando página {Pagina} con orden {Orden}. Clientes en esta página: {Cantidad}",
                numeroPagina, orden ?? "por defecto", paginados.Count);

            return View(paginados);
        }




        // GET: Clientes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cliente = await _clienteRepo.BuscarPorIdAsync(id.Value);
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
            var ahora = DateTime.Now;
            var fechaSinMilisegundos = new DateTime(ahora.Year, ahora.Month, ahora.Day, ahora.Hour, ahora.Minute,0);

            var viewModel = new AddClienteViewModel
            {
                // Asigna la fecha y hora sin milisegundos
                FechaRegistro = fechaSinMilisegundos
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
            _logger.LogInformation("Intentando crear cliente con email: {Email}", viewModel.Email);

            if (ModelState.IsValid)
            {
                bool emailExiste = await _clienteRepo.ExisteEmailAsync(viewModel.Email);
                if (emailExiste)
                {
                    _logger.LogWarning("Creación fallida: Email ya registrado ({Email})", viewModel.Email);
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
                    
                    await _clienteRepo.AgregarAsync(cliente);


                    _logger.LogInformation("Cliente creado exitosamente: ID={Id}, Email={Email}", cliente.Id, cliente.Email);

                    return RedirectToAction(nameof(Index));
                }
            }
            else
            {
                _logger.LogWarning("Validación fallida al crear cliente. Errores: {Errores}",
                    string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            }

            return View(viewModel);
        }




        // Fix for CS1061: Ensure the awaited result of the Task<Cliente> is used before accessing properties.
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var cliente = await _clienteRepo.BuscarPorIdAsync(id); // Await the Task to get the Cliente object.
            if (cliente == null) return NotFound();

            var viewModel = new AddClienteViewModel
            {
                Id = cliente.Id,
                Nombre = cliente.Nombre, // Access properties of the Cliente object.
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
            if (id != viewModel.Id)
            {
                _logger.LogWarning("ID del modelo ({ModeloId}) no coincide con el ID de ruta ({RutaId})", viewModel.Id, id);
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                bool emailExiste = await _clienteRepo.ExisteEmailEnOtroClienteAsync(viewModel.Email, viewModel.Id);

                if (emailExiste)
                {
                    _logger.LogWarning("Actualización fallida: Email ya usado por otro cliente ({Email})", viewModel.Email);
                    ModelState.AddModelError("Email", "El email ya está registrado por otro cliente.");
                }
                else
                {
                    var cliente = await _clienteRepo.BuscarPorIdAsync(id);
                    if (cliente == null)
                    {
                        _logger.LogWarning("Cliente no encontrado con ID={Id} para editar", id);
                        return NotFound();
                    }

                    cliente.Nombre = viewModel.Nombre;
                    cliente.Apellido = viewModel.Apellido;
                    cliente.Email = viewModel.Email;
                    cliente.Telefono = viewModel.Telefono;
                    cliente.FechaRegistro = viewModel.FechaRegistro;

                    await _clienteRepo.ActualizarAsync(cliente);

                    _logger.LogInformation("Cliente ID={Id} actualizado correctamente", id);

                    return RedirectToAction(nameof(Index));
                }
            }
            else
            {
                _logger.LogWarning("Errores de validación al editar cliente ID={Id}: {Errores}",
                    viewModel.Id,
                    string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
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

            var cliente = await _clienteRepo.BuscarPorIdAsync(id.Value);
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
            try
            {
                var cliente = await _clienteRepo.BuscarPorIdAsync(id);
                if (cliente == null)
                {
                    _logger.LogWarning("Intento de eliminar cliente no existente. ID={Id}", id);
                    return NotFound();
                }

                bool tieneCitas = await _citaRepo.ExistenCitasPorClienteAsync(id);
                if (tieneCitas)
                {
                    _logger.LogWarning("No se puede eliminar cliente ID={Id} porque tiene citas asociadas.", id);
                    TempData["Error"] = "No se puede eliminar el cliente porque tiene citas asociadas.";
                    return RedirectToAction(nameof(Index));
                }

                await _clienteRepo.EliminarAsync(id);
                _logger.LogInformation("Cliente eliminado correctamente. ID={Id}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al intentar eliminar cliente ID={Id}", id);
                TempData["Error"] = "Ocurrió un error al eliminar el cliente.";
            }

            return RedirectToAction(nameof(Index));
        }


        private async Task<bool> ClienteExists(int id)
        {
            return await _clienteRepo.ExistePorIdAsync(id);
        }
    }
}
