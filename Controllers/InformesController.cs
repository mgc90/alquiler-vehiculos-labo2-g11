using alquiler_vehiculos_g11.Models.Enums;
using alquiler_vehiculos_g11.Models.ViewModels;
using alquiler_vehiculos_g11.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace alquiler_vehiculos_g11.Controllers;

[Authorize]
public class InformesController : Controller
{
    private const int TamanioPagina = 10;

    private readonly IVehiculoRepository _vehiculoRepository;
    private readonly IAlquilerRepository _alquilerRepository;
    private readonly IInformeClientesRepository _informeClientesRepository;

    public InformesController(
        IVehiculoRepository vehiculoRepository,
        IAlquilerRepository alquilerRepository,
        IInformeClientesRepository informeClientesRepository)
    {
        _vehiculoRepository = vehiculoRepository;
        _alquilerRepository = alquilerRepository;
        _informeClientesRepository = informeClientesRepository;
    }

    [HttpGet]
    public IActionResult Index() => View();

    [HttpGet]
    public async Task<IActionResult> Disponibilidad(
        DateTime? entregaPrevista,
        DateTime? devolucionPrevista,
        int? categoriaVehiculoId,
        int pagina = 1)
    {
        var model = new InformeDisponibilidadViewModel
        {
            EntregaPrevista = entregaPrevista,
            DevolucionPrevista = devolucionPrevista,
            CategoriaVehiculoId = categoriaVehiculoId,
            Categorias = await _vehiculoRepository.ObtenerCategoriasActivasAsync()
        };

        if (entregaPrevista.HasValue != devolucionPrevista.HasValue)
        {
            model.Error = "Ingresá ambas fechas para consultar la disponibilidad.";
            return View(model);
        }

        if (entregaPrevista.HasValue && devolucionPrevista.HasValue)
        {
            if (devolucionPrevista < entregaPrevista)
            {
                model.Error = "La devolución no puede ser anterior a la entrega.";
                return View(model);
            }

            model.Resultado = await _vehiculoRepository.BuscarDisponiblesAsync(
                entregaPrevista.Value,
                devolucionPrevista.Value,
                categoriaVehiculoId,
                pagina,
                TamanioPagina);
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> AlquileresVigentes(
        DateTime? desde,
        DateTime? hasta,
        int pagina = 1)
    {
        var model = new InformeAlquileresVigentesViewModel
        {
            Desde = desde,
            Hasta = hasta
        };

        if (desde.HasValue != hasta.HasValue)
        {
            model.Error = "Ingresá ambas fechas para consultar los alquileres vigentes.";
            return View(model);
        }

        if (desde.HasValue && hasta.HasValue)
        {
            if (hasta < desde)
            {
                model.Error = "La fecha final no puede ser anterior a la inicial.";
                return View(model);
            }

            model.Resultado = await _alquilerRepository.ObtenerVigentesAsync(
                desde.Value,
                hasta.Value,
                pagina,
                TamanioPagina);
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Vehiculos(EstadoOperativoVehiculo? estado, int pagina = 1)
    {
        var model = new InformeVehiculosViewModel
        {
            Estado = estado,
            Resultado = await _vehiculoRepository.ObtenerPorEstadoAsync(estado, pagina, TamanioPagina)
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> VehiculosPorCategoria(int? categoriaVehiculoId, int pagina = 1)
    {
        var model = new InformeVehiculosCategoriaViewModel
        {
            CategoriaVehiculoId = categoriaVehiculoId,
            Categorias = await _vehiculoRepository.ObtenerCategoriasActivasAsync(),
            Resultado = await _vehiculoRepository.ObtenerPorCategoriaAsync(
                categoriaVehiculoId,
                pagina,
                TamanioPagina)
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> MasAlquilados(int pagina = 1)
    {
        var hasta = DateTime.Today;
        var desde = hasta.AddDays(-364);
        var resultado = await _vehiculoRepository.ObtenerMasAlquiladosAsync(
            desde,
            hasta,
            pagina,
            TamanioPagina);

        return View(new InformeMasAlquiladosViewModel { Resultado = resultado });
    }

    [HttpGet]
    public async Task<IActionResult> SinAlquileres(int dias = 30, int pagina = 1)
    {
        var model = new InformeSinAlquileresViewModel { Dias = dias };
        if (dias < 1 || dias > 3650)
        {
            model.Error = "Ingresá un período entre 1 y 3650 días.";
            return View(model);
        }

        model.Resultado = await _vehiculoRepository.ObtenerSinAlquileresDesdeAsync(
            DateTime.Today.AddDays(-(dias - 1)),
            pagina,
            TamanioPagina);

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> QueFinalizan(int dias = 7, int pagina = 1)
    {
        var model = new InformeQueFinalizanViewModel { Dias = dias };
        if (dias < 1 || dias > 3650)
        {
            model.Error = "Ingresá un período entre 1 y 3650 días.";
            return View(model);
        }

        model.Resultado = await _alquilerRepository.ObtenerQueFinalizanAsync(
            DateTime.Today,
            DateTime.Today.AddDays(dias),
            pagina,
            TamanioPagina);

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> LicenciasPorVencer(int dias = 30, int pagina = 1)
    {
        var model = new InformeLicenciasViewModel { Dias = dias };
        if (dias < 1 || dias > 3650)
        {
            model.Error = "Ingresá un período entre 1 y 3650 días.";
            return View(model);
        }

        model.Resultado = await _informeClientesRepository.ObtenerLicenciasPorVencerAsync(
            DateTime.Today,
            DateTime.Today.AddDays(dias),
            pagina,
            TamanioPagina);

        return View(model);
    }
}
