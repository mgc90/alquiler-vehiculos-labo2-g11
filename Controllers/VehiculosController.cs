using alquiler_vehiculos_g11.Models;
using alquiler_vehiculos_g11.Models.Enums;
using alquiler_vehiculos_g11.Models.ViewModels;
using alquiler_vehiculos_g11.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace alquiler_vehiculos_g11.Controllers;

[Authorize(Roles = "Administrador")]
public class VehiculosController : Controller
{
    private const int TamPagina = 10;

    private readonly IVehiculoRepository _vehiculoRepository;
    private readonly ICategoriaVehiculoRepository _categoriaRepository;

    private readonly IImagenRepository _imagenRepository;

    public VehiculosController(
        IVehiculoRepository vehiculoRepository,
        ICategoriaVehiculoRepository categoriaRepository,
        IImagenRepository imagenRepository)
    {
        _vehiculoRepository = vehiculoRepository;
        _categoriaRepository = categoriaRepository;
        _imagenRepository = imagenRepository;
    }

    [HttpGet]
    public IActionResult Index() => View();

    [HttpGet]
    public async Task<IActionResult> Listar(string? termino, int pagina = 1)
    {
        var resultado = await _vehiculoRepository.BuscarAsync(
            termino?.Trim() ?? string.Empty,
            pagina,
            TamPagina);

        var portadas = await _imagenRepository.ObtenerPortadasPorVehiculosAsync(
        resultado.Elementos.Select(v => v.Id));
        

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
                vehiculo.KilometrajeActual,
                EstadoOperativo = vehiculo.EstadoOperativo.ToString(),
                vehiculo.CategoriaVehiculoId,
                Categoria = vehiculo.CategoriaVehiculo?.Nombre,
                PortadaUrl = portadas.GetValueOrDefault(vehiculo.Id)
            })
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear([FromBody] VehiculoFormularioViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return ErroresDeValidacion();
        }

        if (!TryEstadoOperativo(model.EstadoOperativo, out var estado))
        {
            ModelState.AddModelError(nameof(model.EstadoOperativo), "Estado operativo inválido.");
            return ErroresDeValidacion();
        }

        var patente = model.Patente.Trim().ToUpperInvariant();

        if (await _vehiculoRepository.ExistePatenteAsync(patente))
        {
            ModelState.AddModelError(nameof(model.Patente), "Ya existe un vehículo con esa patente.");
            return ErroresDeValidacion();
        }

        if (!await CategoriaActivaAsync(model.CategoriaVehiculoId))
        {
            ModelState.AddModelError(
                nameof(model.CategoriaVehiculoId),
                "La categoría seleccionada no existe o está inactiva.");
            return ErroresDeValidacion();
        }

        var vehiculo = new Vehiculo
        {
            Patente = patente,
            Marca = model.Marca.Trim(),
            Modelo = model.Modelo.Trim(),
            KilometrajeActual = model.KilometrajeActual,
            CategoriaVehiculoId = model.CategoriaVehiculoId,
            EstadoOperativo = estado
        };

        await _vehiculoRepository.AgregarAsync(vehiculo);

        return Json(new { vehiculo.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Actualizar([FromBody] VehiculoFormularioViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return ErroresDeValidacion();
        }

        var vehiculo = await _vehiculoRepository.ObtenerPorIdAsync(model.Id);
        if (vehiculo is null)
        {
            return NotFound(new { mensaje = "El vehículo no existe." });
        }

        if (!TryEstadoOperativo(model.EstadoOperativo, out var estado))
        {
            ModelState.AddModelError(nameof(model.EstadoOperativo), "Estado operativo inválido.");
            return ErroresDeValidacion();
        }

        var patente = model.Patente.Trim().ToUpperInvariant();

        if (await _vehiculoRepository.ExistePatenteAsync(patente, vehiculo.Id))
        {
            ModelState.AddModelError(nameof(model.Patente), "Ya existe un vehículo con esa patente.");
            return ErroresDeValidacion();
        }

        if (!await CategoriaActivaAsync(model.CategoriaVehiculoId))
        {
            ModelState.AddModelError(
                nameof(model.CategoriaVehiculoId),
                "La categoría seleccionada no existe o está inactiva.");
            return ErroresDeValidacion();
        }

        vehiculo.Patente = patente;
        vehiculo.Marca = model.Marca.Trim();
        vehiculo.Modelo = model.Modelo.Trim();
        vehiculo.KilometrajeActual = model.KilometrajeActual;
        vehiculo.CategoriaVehiculoId = model.CategoriaVehiculoId;
        vehiculo.EstadoOperativo = estado;

        await _vehiculoRepository.ActualizarAsync(vehiculo);

        return Json(new { vehiculo.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Baja(int id)
    {
        var vehiculo = await _vehiculoRepository.ObtenerPorIdAsync(id);
        if (vehiculo is null)
        {
            return NotFound(new { mensaje = "El vehículo no existe." });
        }

        vehiculo.EstadoOperativo = EstadoOperativoVehiculo.Baja;
        await _vehiculoRepository.ActualizarAsync(vehiculo);

        return Json(new
        {
            vehiculo.Id,
            EstadoOperativo = vehiculo.EstadoOperativo.ToString()
        });
    }

    [HttpGet]
    public IActionResult Details(int id) => View();

    [HttpGet]
    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Vehiculo vehiculo) => View();

    [HttpGet]
    public IActionResult Edit(int id) => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Vehiculo vehiculo) => View();

    [HttpGet]
    public IActionResult Delete(int id) => View();

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id) => View("Delete");

    private async Task<bool> CategoriaActivaAsync(int categoriaId)
    {
        var categoria = await _categoriaRepository.ObtenerPorIdAsync(categoriaId);
        return categoria is { Activa: true };
    }

    private static bool TryEstadoOperativo(
        string? valor,
        out EstadoOperativoVehiculo estado) =>
        Enum.TryParse(valor, ignoreCase: true, out estado) && Enum.IsDefined(estado);

    private BadRequestObjectResult ErroresDeValidacion() =>
        BadRequest(new { errores = ErroresDeModelo(ModelState) });

    private static Dictionary<string, string[]> ErroresDeModelo(
        ModelStateDictionary modelState) =>
        modelState
            .Where(entrada => entrada.Value?.Errors.Count > 0)
            .GroupBy(entrada => CampoDe(entrada.Key))
            .ToDictionary(
                grupo => grupo.Key,
                grupo => grupo
                    .SelectMany(entrada => entrada.Value!.Errors)
                    .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                        ? "Valor inválido."
                        : error.ErrorMessage)
                    .Distinct()
                    .ToArray());

    private static string CampoDe(string clave)
    {
        var indice = clave.LastIndexOf('.');
        return indice >= 0 ? clave[(indice + 1)..] : clave;
    }
}
