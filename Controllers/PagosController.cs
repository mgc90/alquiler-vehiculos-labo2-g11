
using System.Security.Claims;
using alquiler_vehiculos_g11.Models.ViewModels;
using alquiler_vehiculos_g11.Repositories;
using alquiler_vehiculos_g11.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace alquiler_vehiculos_g11.Controllers;

[Authorize]
public class PagosController : Controller
{
    private readonly IPagoService _pagoService;
    private readonly IPagoRepository _pagoRepository;
    private readonly IAlquilerRepository _alquilerRepository;

    public PagosController(
        IPagoService pagoService,
        IPagoRepository pagoRepository,
        IAlquilerRepository alquilerRepository)
    {
        _pagoService = pagoService;
        _pagoRepository = pagoRepository;
        _alquilerRepository = alquilerRepository;
    }

    [HttpGet]
    public IActionResult Index() => View();

    [HttpGet]
    public IActionResult Details(int id) => View();

    [HttpGet]
    public async Task<IActionResult> Registrar(int alquilerId)
    {
        var alquiler = await _alquilerRepository.ObtenerPorIdAsync(alquilerId);

        if (alquiler == null)
        {
            return NotFound();
        }

        var modelo = new PagoFormularioViewModel
        {
            AlquilerId = alquilerId
        };

        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Registrar(PagoFormularioViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        if (!int.TryParse(
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            out var usuarioId))
        {
            return Forbid();
        }

        try
        {
            await _pagoService.RegistrarAsync(
                modelo.AlquilerId,
                usuarioId,
                modelo.Importe,
                modelo.Tipo!.Value,
                modelo.MedioPago);

            TempData["Mensaje"] = "El pago se registró correctamente.";

            return RedirectToAction(
                "Details",
                "Alquileres",
                new { id = modelo.AlquilerId });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }

        return View(modelo);
    }

    [HttpGet]
    public async Task<IActionResult> Anular(int id)
    {
        var pago = await _pagoRepository.ObtenerPorIdAsync(id);

        if (pago == null)
        {
            return NotFound();
        }

        var modelo = new PagoAnulacionViewModel
        {
            Id = pago.Id,
            AlquilerId = pago.AlquilerId,
            Importe = pago.Importe,
            Tipo = pago.Tipo,
            MedioPago = pago.MedioPago,
            Fecha = pago.Fecha
        };

        return View(modelo);
    }

    [HttpPost, ActionName("Anular")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AnularConfirmado(int id)
    {
        var pago = await _pagoRepository.ObtenerPorIdAsync(id);

        if (pago == null)
        {
            return NotFound();
        }

        if (!int.TryParse(
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            out var usuarioId))
        {
            return Forbid();
        }

        try
        {
            await _pagoService.AnularAsync(id, usuarioId);

            TempData["Mensaje"] = "El pago se anuló correctamente.";

            return RedirectToAction(
                "Details",
                "Alquileres",
                new { id = pago.AlquilerId });
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;

            return RedirectToAction(
                "Details",
                "Alquileres",
                new { id = pago.AlquilerId });
        }
    }
}
