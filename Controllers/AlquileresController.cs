using System.Security.Claims;
using alquiler_vehiculos_g11.Models;
using alquiler_vehiculos_g11.Models.ViewModels;
using alquiler_vehiculos_g11.Repositories;
using alquiler_vehiculos_g11.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace alquiler_vehiculos_g11.Controllers;

[Authorize]
public class AlquileresController : Controller
{
    private const int TamPagina = 10;

    private readonly IAlquilerService _alquilerService;
    private readonly IVehiculoRepository _vehiculoRepository;
    private readonly IClienteRepository _clienteRepository;

    public AlquileresController(
        IAlquilerService alquilerService,
        IVehiculoRepository vehiculoRepository,
        IClienteRepository clienteRepository)
    {
        _alquilerService = alquilerService;
        _vehiculoRepository = vehiculoRepository;
        _clienteRepository = clienteRepository;
    }

    [HttpGet]
    public IActionResult Index() => View();

    [HttpGet]
    public IActionResult Details(int id) => View();

    [HttpGet]
    public async Task<IActionResult> NuevaReserva()
    {
        var model = new NuevaReservaViewModel
        {
            Categorias = await _vehiculoRepository.ObtenerCategoriasActivasAsync()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> NuevaReserva(NuevaReservaViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Categorias = await _vehiculoRepository.ObtenerCategoriasActivasAsync();
            return View(model);
        }

        var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            var alquiler = await _alquilerService.CrearReservaAsync(
                model.ClienteId,
                model.VehiculoId,
                usuarioId,
                model.EntregaPrevista,
                model.DevolucionPrevista);

            TempData["Mensaje"] = $"Reserva #{alquiler.Id} registrada para el {alquiler.EntregaPrevista:dd/MM/yyyy}.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            model.Categorias = await _vehiculoRepository.ObtenerCategoriasActivasAsync();
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> BuscarClientes(string? termino, int pagina = 1)
    {
        var resultado = await _clienteRepository.BuscarPorTerminoAsync(termino ?? string.Empty, pagina, TamPagina);

        return Json(new
        {
            resultado.TotalRegistros,
            resultado.Pagina,
            resultado.TamanioPagina,
            Elementos = resultado.Elementos.Select(cliente => new
            {
                cliente.Id,
                cliente.Nombre,
                cliente.Apellido,
                cliente.Dni,
                VencimientoLicencia = cliente.VencimientoLicencia.ToString("yyyy-MM-dd")
            })
        });
    }

    [HttpGet]
    public async Task<IActionResult> Disponibles(
        DateTime entregaPrevista,
        DateTime devolucionPrevista,
        int? categoriaVehiculoId,
        int pagina = 1)
    {
        try
        {
            var resultado = await _alquilerService.BuscarDisponiblesAsync(
                entregaPrevista,
                devolucionPrevista,
                categoriaVehiculoId,
                pagina,
                TamPagina);

            return Json(new
            {
                resultado.TotalRegistros,
                resultado.Pagina,
                resultado.TamanioPagina,
                Elementos = resultado.Elementos.Select(vehiculo => new
                {
                    vehiculo.Id,
                    vehiculo.Patente,
                    vehiculo.Marca,
                    vehiculo.Modelo,
                    Categoria = vehiculo.CategoriaVehiculo?.Nombre,
                    PrecioDiario = vehiculo.CategoriaVehiculo?.PrecioDiario
                })
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Mensaje = ex.Message });
        }
    }

    [HttpGet]
    public IActionResult Entregar(int id) => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Entregar(int id, DateTime entregaReal) => View();

    [HttpGet]
    public IActionResult Devolver(int id) => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Devolver(int id, DateTime devolucionReal, string? observaciones) => View();

    [HttpGet]
    public IActionResult Cancelar(int id) => View();

    [HttpPost, ActionName("Cancelar")]
    [ValidateAntiForgeryToken]
    public IActionResult CancelarConfirmado(int id) => View("Cancelar");

    [HttpGet]
    public IActionResult Extender(int id) => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Extender(int id, DateTime entregaPrevista, DateTime devolucionPrevista) => View();
}
