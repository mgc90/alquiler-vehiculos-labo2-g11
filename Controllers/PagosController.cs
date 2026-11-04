
using System.Security.Claims;
using alquiler_vehiculos_g11.Models;
using alquiler_vehiculos_g11.Models.Enums;
using alquiler_vehiculos_g11.Models.ViewModels;
using alquiler_vehiculos_g11.Repositories;
using alquiler_vehiculos_g11.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace alquiler_vehiculos_g11.Controllers;

[Authorize]
public class PagosController : Controller
{
    private const int TamPagina = 10;

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
    public async Task<IActionResult> Index(
        EstadoPago? estado,
        int? alquilerId,
        int pagina = 1)
    {
        var modelo = new PagosIndexViewModel
        {
            Estado = estado,
            AlquilerId = alquilerId,
            Resultado = await _pagoRepository.BuscarAsync(
                estado,
                alquilerId,
                pagina,
                TamPagina)
        };

        return View(modelo);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var pago = await _pagoRepository.ObtenerPorIdAsync(id);

        if (pago == null)
        {
            return NotFound();
        }

        var alquiler = await _alquilerRepository.ObtenerDetalleAsync(
            pago.AlquilerId);

        var pagosVigentes = alquiler == null
            ? 0m
            : CalculoImportesAlquiler.PagosVigentes(alquiler.Pagos);

        var modelo = new PagoDetalleViewModel
        {
            Pago = pago,
            Alquiler = alquiler,
            PagosVigentes = pagosVigentes,
            SaldoPendiente = alquiler == null
                ? 0m
                : alquiler.TotalExigible - pagosVigentes
        };

        return View(modelo);
    }

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

        await CompletarDatosPagoAsync(modelo);

        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Registrar(PagoFormularioViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            await CompletarDatosPagoAsync(modelo);
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

        await CompletarDatosPagoAsync(modelo);

        return View(modelo);
    }

    private async Task CompletarDatosPagoAsync(PagoFormularioViewModel modelo)
    {
        var alquiler = await _alquilerRepository.ObtenerPorIdAsync(modelo.AlquilerId);

        if (alquiler == null)
        {
            return;
        }

        var totalPagado = await _pagoRepository
            .ObtenerTotalVigentePorAlquilerAsync(modelo.AlquilerId);

        var cargosPagados = await _pagoRepository
            .ObtenerTotalVigentePorTiposAsync(
                modelo.AlquilerId,
                TipoPago.Multa,
                TipoPago.Recargo);

        modelo.ImporteOriginal = alquiler.ImporteTotal;
        modelo.Multa = alquiler.Multa;
        modelo.ImporteExigible = alquiler.TotalExigible;
        modelo.SaldoPendiente = Math.Max(
            0m,
            alquiler.TotalExigible - totalPagado);
        modelo.CargosPendientes = Math.Max(
            0m,
            Math.Min(alquiler.Multa - cargosPagados, modelo.SaldoPendiente));
        modelo.EstadoAlquiler = alquiler.Estado;
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
