using System.Security.Claims;
using alquiler_vehiculos_g11.Models;
using alquiler_vehiculos_g11.Models.ViewModels;
using alquiler_vehiculos_g11.Repositories;
using alquiler_vehiculos_g11.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using alquiler_vehiculos_g11.Models.Enums;

namespace alquiler_vehiculos_g11.Controllers;

[Authorize]
public class AlquileresController : Controller
{
    private const int TamPagina = 10;

    private readonly IAlquilerService _alquilerService;
    private readonly IAlquilerRepository _alquilerRepository;
    private readonly IVehiculoRepository _vehiculoRepository;
    private readonly IClienteRepository _clienteRepository;

    public AlquileresController(
        IAlquilerService alquilerService,
        IAlquilerRepository alquilerRepository,
        IVehiculoRepository vehiculoRepository,
        IClienteRepository clienteRepository)
    {
        _alquilerService = alquilerService;
        _alquilerRepository = alquilerRepository;
        _vehiculoRepository = vehiculoRepository;
        _clienteRepository = clienteRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        EstadoAlquiler? estado,
        int? clienteId,
        int? vehiculoId,
        int pagina = 1)
    {
        var modelo = new AlquileresIndexViewModel
        {
            Estado = estado,
            ClienteId = clienteId,
            VehiculoId = vehiculoId,
            Resultado = await _alquilerRepository.BuscarAsync(
                estado,
                clienteId,
                vehiculoId,
                pagina,
                TamPagina)
        };

        return View(modelo);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var alquiler = await _alquilerService.ObtenerDetalleAsync(id);

        if (alquiler == null)
        {
            return NotFound();
        }

        return View(alquiler);
    }

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
        int pagina = 1,
        int? alquilerExcluirId = null,
        int? vehiculoId = null)
    {
        try
        {
            if (alquilerExcluirId.HasValue)
            {
                var alquilerOriginal = await _alquilerService.ObtenerPorIdAsync(
                    alquilerExcluirId.Value);

                if (alquilerOriginal == null
                    || (alquilerOriginal.Estado != EstadoAlquiler.Reservado
                        && alquilerOriginal.Estado != EstadoAlquiler.EnCurso)
                    || entregaPrevista.Date != alquilerOriginal.DevolucionPrevista.Date)
                {
                    return BadRequest(new
                    {
                        Mensaje = "El alquiler indicado no puede utilizarse para esta extensión."
                    });
                }
            }
            if (vehiculoId.HasValue)
            {
                var vehiculo = await _vehiculoRepository.ObtenerPorIdAsync(
                    vehiculoId.Value);
                if (vehiculo == null)
                {
                    return NotFound(new
                    {
                        Mensaje = "El vehículo indicado no existe."
                    });
                }

                var disponible = await _vehiculoRepository.EsDisponibleAsync(
                    vehiculo.Id,
                    entregaPrevista,
                    devolucionPrevista,
                    alquilerExcluirId);

                return Json(new
                {
                    Disponible = disponible,
                    Vehiculo = new
                    {
                        vehiculo.Id,
                        vehiculo.Patente,
                        vehiculo.Marca,
                        vehiculo.Modelo,
                        Categoria = vehiculo.CategoriaVehiculo?.Nombre,
                        PrecioDiario = vehiculo.CategoriaVehiculo?.PrecioDiario
                    }
                });
            }

            // Si no se solicita un vehículo puntual, se mantiene la búsqueda paginada.
            var resultado = await _alquilerService.BuscarDisponiblesAsync(
                entregaPrevista,
                devolucionPrevista,
                categoriaVehiculoId,
                pagina,
                TamPagina,
                alquilerExcluirId);

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

        var hoy = DateTime.Now;

        var diasContratados = CalculoImportesAlquiler.DiasContratados(
            a.EntregaPrevista,
            a.DevolucionPrevista);

        var diasUso = a.EntregaReal.HasValue
            ? CalculoImportesAlquiler.DiasUso(a.EntregaReal.Value, hoy)
            : 0;

        var esCancelacionAnticipada = a.EntregaReal.HasValue
            && CalculoImportesAlquiler.EsDevolucionAnticipada(
                a.DevolucionPrevista,
                hoy);

        var diasRestantes = esCancelacionAnticipada
            ? Math.Max(0, diasContratados - diasUso)
            : 0;

        var porcentajeMulta = esCancelacionAnticipada
            ? CalculoImportesAlquiler.PorcentajeMultaDiasRestantes(
                diasContratados,
                diasUso)
            : 0m;

        var multaCancelacion = esCancelacionAnticipada
            ? CalculoImportesAlquiler.MultaDiasRestantes(
                a.PrecioAcordado,
                diasContratados,
                diasUso)
            : 0m;

        var importeUso = esCancelacionAnticipada
            ? a.PrecioAcordado * diasUso
            : 0m;

        var diasDemora = esCancelacionAnticipada
            ? 0
            : CalculoImportesAlquiler.DiasDemora(
                a.DevolucionPrevista,
                hoy);

        return Json(new
        {
            cliente = $"{a.Cliente!.Apellido}, {a.Cliente.Nombre}",
            dni = $"{a.Cliente.Dni}",
            vehiculo = $"{a.Vehiculo!.Marca} {a.Vehiculo.Modelo} ({a.Vehiculo.Patente})",
            kilometraje = a.Vehiculo.KilometrajeActual,
            entregaReal = a.EntregaReal?.ToString("dd/MM/yyyy HH:mm"),
            devolucionPrevista = a.DevolucionPrevista.ToString("dd/MM/yyyy"),
            precioAcordado = a.PrecioAcordado,
            importeOriginal = a.ImporteTotal,
            importeUso,
            pagado = CalculoImportesAlquiler.PagosVigentes(a.Pagos),
            esCancelacionAnticipada,
            diasUso,
            diasRestantes,
            porcentajeMulta,
            multaCancelacion,
            diasDemora,
            recargo = diasDemora * a.PrecioAcordado
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
    public async Task<IActionResult> Cancelar(int id)
    {
        var alquiler = await _alquilerService.ObtenerDetalleAsync(id);

        if (alquiler == null)
        {
            return NotFound();
        }

        if (alquiler.Estado != EstadoAlquiler.Reservado ||
            alquiler.EntregaReal.HasValue)
        {
            TempData["Error"] = "Solo se pueden cancelar reservas cuyo vehículo todavía no fue entregado.";

            return RedirectToAction(nameof(Details), new { id });
        }

        return View(alquiler);
    }

    [HttpPost, ActionName("Cancelar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelarConfirmado(int id)
    {
        var usuarioIdClaim =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(usuarioIdClaim, out var usuarioId))
        {
            return Forbid();
        }

        try
        {
            await _alquilerService.CancelarAsync(id, usuarioId);
            TempData["Mensaje"] = "El alquiler fue cancelado correctamente.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Cancelar), new { id });
        }
    }



    [HttpGet]
    public async Task<IActionResult> Extender(int id)
    {
        var alquiler = await _alquilerService.ObtenerPorIdAsync(id);

        if (alquiler == null)
        {
            return NotFound();
        }

        if (alquiler.Estado != EstadoAlquiler.Reservado &&
            alquiler.Estado != EstadoAlquiler.EnCurso)
        {
            TempData["Error"] = "Solo se pueden extender alquileres reservados o en curso.";
            return RedirectToAction(nameof(Index));
        }

        if (alquiler.DevolucionPrevista.Date < DateTime.Today)
        {
            TempData["Error"] = "El alquiler está vencido: primero registre la devolución física para calcular la demora y los daños, y luego cree una nueva reserva.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var model = new ExtenderAlquilerViewModel
        {
            AlquilerId = alquiler.Id,
            ClienteNombre = $"{alquiler.Cliente?.Nombre} {alquiler.Cliente?.Apellido}",
            VehiculoOriginal = $"{alquiler.Vehiculo?.Marca} {alquiler.Vehiculo?.Modelo}",
            EstadoOriginal = alquiler.Estado,
            EntregaExtension = alquiler.DevolucionPrevista.Date,
            DevolucionPrevista = alquiler.DevolucionPrevista.Date.AddDays(1),
            VehiculoId = alquiler.VehiculoId,
            Categorias = await _vehiculoRepository.ObtenerCategoriasActivasAsync()
        };
        ViewData["VehiculoOriginalId"] = alquiler.VehiculoId;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Extender(ExtenderAlquilerViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var alquilerOriginal = await _alquilerService.ObtenerPorIdAsync(model.AlquilerId);

            if (alquilerOriginal == null)
            {
                return NotFound();
            }
            model.ClienteNombre = $"{alquilerOriginal.Cliente?.Nombre} {alquilerOriginal.Cliente?.Apellido}";
            model.VehiculoOriginal = $"{alquilerOriginal.Vehiculo?.Marca} {alquilerOriginal.Vehiculo?.Modelo}";
            model.EstadoOriginal = alquilerOriginal.Estado;
            model.EntregaExtension = alquilerOriginal.DevolucionPrevista.Date;
            model.Categorias = await _vehiculoRepository.ObtenerCategoriasActivasAsync();
            ViewData["VehiculoOriginalId"] = alquilerOriginal.VehiculoId;
            return View(model);
        }

        var usuarioId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            var nuevoAlquiler = await _alquilerService.ExtenderAsync(
                model.AlquilerId,
                model.VehiculoId,
                usuarioId,
                model.EntregaExtension,
                model.DevolucionPrevista);

            TempData["Mensaje"] = $"Extensión registrada correctamente. Nuevo alquiler #{nuevoAlquiler.Id}.";

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex) when (
            ex is ArgumentException or InvalidOperationException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            var alquilerOriginal = await _alquilerService.ObtenerPorIdAsync(model.AlquilerId);
            if (alquilerOriginal != null)
            {
                model.ClienteNombre = $"{alquilerOriginal.Cliente?.Nombre} {alquilerOriginal.Cliente?.Apellido}";
                model.VehiculoOriginal = $"{alquilerOriginal.Vehiculo?.Marca} {alquilerOriginal.Vehiculo?.Modelo}";
                model.EstadoOriginal = alquilerOriginal.Estado;
                model.EntregaExtension = alquilerOriginal.DevolucionPrevista.Date;
                ViewData["VehiculoOriginalId"] = alquilerOriginal.VehiculoId;
            }
            model.Categorias = await _vehiculoRepository.ObtenerCategoriasActivasAsync();
            return View(model);
        }
    }

}
