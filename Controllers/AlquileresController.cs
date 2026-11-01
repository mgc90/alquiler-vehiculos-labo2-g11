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
    public async Task<IActionResult> Entregar(int id)
    {
        var CategoriasActivas = await _vehiculoRepository.ObtenerCategoriasActivasAsync();

        return View(CategoriasActivas);
    }

    [HttpGet]
    public async Task<IActionResult> ReservadosPorCategoria(int categoriaId)
    {
        var reservas = await _alquilerService.ObtenerReservadosPorCategoriaAsync(categoriaId);

        return Json(reservas.Select(a => new
        {
            alquilerId = a.Id,
            texto = $"{a.Vehiculo!.Patente} - {a.Vehiculo.Marca} {a.Vehiculo.Modelo}"
        }));
    }

    [HttpGet]
    public async Task <IActionResult> EnCursoPorCategoria(int categoriaId)
    {
        var enCurso = await _alquilerService.ObtenerEnCursoPorCategoriaAsync(categoriaId);

        return Json(enCurso.Select(a => new
        {
            alquilerId = a.Id,
            texto = $"{a.Vehiculo!.Patente} - {a.Vehiculo.Marca} {a.Vehiculo.Modelo}"
        }));
    }

    [HttpGet]
    public async Task<IActionResult> DetalleReserva(int alquilerId)
    {
        var a = await _alquilerService.ObtenerReservadoParaEntregaAsync(alquilerId);
        if (a == null) return NotFound();

        return Json(new
        {
            cliente = $"{a.Cliente!.Apellido}, {a.Cliente.Nombre}",
            dni =  $"{a.Cliente.Dni}",
            vehiculo = $"{a.Vehiculo!.Marca} {a.Vehiculo.Modelo} ({a.Vehiculo.Patente})",
            kilometraje = a.Vehiculo.KilometrajeActual,
            entregaPrevista = a.EntregaPrevista.ToString("dd/MM/yyyy HH:mm"),
            devolucionPrevista = a.DevolucionPrevista.ToString("dd/MM/yyyy HH:mm"),
            precioAcordado = a.PrecioAcordado,
        });
    }

    [HttpGet]
    public async Task<IActionResult> DetalleReservaEnCurso(int alquilerId)
    {
        var a = await _alquilerService.ObtenerEnCursoParaDevolucionAsync(alquilerId);
        if (a == null) return NotFound();

        var limite = a.DevolucionPrevista.Date.AddDays(1);
        var demora = DateTime.Now - limite;
        var diasDemora = demora > TimeSpan.Zero ? (int)Math.Ceiling(demora.TotalDays): 0;

        return Json(new
        {
            cliente = $"{a.Cliente!.Apellido}, {a.Cliente.Nombre}",
            dni = $"{a.Cliente.Dni}",
            vehiculo = $"{a.Vehiculo!.Marca} {a.Vehiculo.Modelo} ({a.Vehiculo.Patente})",
            kilometraje = a.Vehiculo.KilometrajeActual,
            entregaReal = a.EntregaReal?.ToString("dd/MM/yyyy HH:mm"),
            devolucionPrevista = a.DevolucionPrevista.ToString("dd/MM/yyyy"),
            diasDemora,
            recargo = diasDemora * a.PrecioAcordado,
            importeTotal = a.ImporteTotal,
            precioAcordado = a.PrecioAcordado
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Entregar(int alquilerId, DateTime entregaReal)
    {

        if (!ModelState.IsValid)
        {
            var CategoriasActivas = await _vehiculoRepository.ObtenerCategoriasActivasAsync();
            return View(CategoriasActivas);
        }

        try
        {
            entregaReal = DateTime.Now;
            var alquiler = await _alquilerService.EntregarVehiculoAsync(alquilerId, entregaReal);

            TempData["Mensaje"] = $"Reserva #{alquilerId} en curso hasta el {alquiler.DevolucionPrevista:dd/MM/yyyy}.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            var CategoriasActivas = await _vehiculoRepository.ObtenerCategoriasActivasAsync();
            return View(CategoriasActivas);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Devolver(int id)
    {

        var model = new DevolucionVehiculoViewModel
        {
            Categorias = await _vehiculoRepository.ObtenerCategoriasActivasAsync()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Devolver(int alquilerId, int kilometrajeDevolucion, decimal costosDanios, string? observaciones)
    {
        try
        {
            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var devolucionReal = DateTime.Now;

            var alquiler = await _alquilerService.RegistrarDevolucionAsync(
                alquilerId, usuarioId, devolucionReal, kilometrajeDevolucion, costosDanios, observaciones);

            return RedirectToAction(nameof(DetalleDevolucion), new { id = alquiler.Id });
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);

            return View(new DevolucionVehiculoViewModel
            {
                Categorias = await _vehiculoRepository.ObtenerCategoriasActivasAsync()
            });
        }
    }

    [HttpGet]
    public async Task<IActionResult> DetalleDevolucion(int id)
    {
        var alquiler = await _alquilerService.ObtenerDetalleAsync(id);
        if (alquiler == null) return NotFound();

        return View(alquiler);
    }

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
