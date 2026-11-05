using alquiler_vehiculos_g11.Models;
using alquiler_vehiculos_g11.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace alquiler_vehiculos_g11.Controllers;

[Authorize]
public class ImagenesController : Controller
{

    private readonly IImagenRepository _imagenesRepository;

    private readonly IVehiculoRepository _vehiculoRepository;

    private readonly IWebHostEnvironment _env;

    private static readonly string[] ExtensionesPermitidas = { ".jpg", ".jpeg", ".png", ".webp" };
    private const long MaxBytesImagen = 5 * 1024 * 1024;
    private const int MaxImagenes = 6;

    public ImagenesController(
        IImagenRepository imagenesRepository,
        IVehiculoRepository vehiculoRepository,
        IWebHostEnvironment env)
    {
        _imagenesRepository = imagenesRepository;
        _vehiculoRepository = vehiculoRepository;
        _env = env;
    }

    [HttpGet]
    public IActionResult Index() => View();

    [HttpGet]
    public IActionResult Details(int id) => View();

    [HttpGet]
    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Imagen imagen) => View();

    [HttpGet]
    public IActionResult Edit(int id) => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Imagen imagen) => View();

    [HttpGet]
    public IActionResult Delete(int id) => View();

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id) => View("Delete");

      [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GuardarPortada(int vehiculoId, IFormFile? imagen)
    {
        if (imagen is null || imagen.Length == 0)
            return BadRequest(new { error = "Elegí una imagen." });

        var error = Validar(imagen);
        if (error is not null)
            return BadRequest(new { error });

        var nuevaUrl = await GuardarAsync(imagen, "portadas");
        try
        {
            var existente = await _imagenesRepository.ObtenerPortadaPorVehiculoAsync(vehiculoId);
            var vehiculo = await _vehiculoRepository.ObtenerPorIdAsync(vehiculoId);

            if (vehiculo is null)
            {
                return NotFound(new { error = "El vehículo no existe." });
            }

            string? urlVieja = null;

            // Si no hay portada del vehículo, subirla y guardarla
            if (existente is null)
            {
                await _imagenesRepository.AgregarAsync(new Imagen
                {
                    VehiculoId = vehiculoId,
                    Titulo = "Portada",
                    Url = nuevaUrl,
                    Descripcion = $"Portada para {vehiculo.Marca} {vehiculo.Modelo}({vehiculo.CategoriaVehiculo})"
                });
            }
            else
            // Reemplazar la portada existente por una nueva
            {
                urlVieja = existente.Url;
                existente.Url = nuevaUrl;
                await _imagenesRepository.ActualizarAsync(existente);
            }

            Eliminar(urlVieja); 
            return Json(new { url = nuevaUrl });
        }
        catch
        {
            Eliminar(nuevaUrl); 
            throw;
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarPortada(int vehiculoId)
    {
        var portada = await _imagenesRepository.ObtenerPortadaPorVehiculoAsync(vehiculoId);
        if (portada is null) return NotFound(new { error = "El vehículo no tiene portada." });

        var url = portada.Url;
        await _imagenesRepository.EliminarAsync(portada);
        Eliminar(url);

        return Ok();
    }

    public string? Validar(IFormFile archivo)
    {
        var ext = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        if (!ExtensionesPermitidas.Contains(ext)) return "Solo se permiten formatos jpg, jpeg, png o webp.";
        if (archivo.Length > MaxBytesImagen) return "La imagen supera los 5 MB.";
        return null;
    }

    public async Task<string> GuardarAsync(IFormFile archivo, string subcarpeta)
    {
        var ext = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        var nombre = $"{Guid.NewGuid()}{ext}";
        var carpeta = Path.Combine(_env.WebRootPath, "uploads", subcarpeta);
        Directory.CreateDirectory(carpeta);

        await using var stream = new FileStream(Path.Combine(carpeta, nombre), FileMode.Create);
        await archivo.CopyToAsync(stream);

        return $"/uploads/{subcarpeta}/{nombre}";
    }

    public void Eliminar(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return;

        var uploads = Path.GetFullPath(Path.Combine(_env.WebRootPath, "uploads"));
        var ruta = Path.GetFullPath(Path.Combine(
            _env.WebRootPath, url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)));

        if (ruta.StartsWith(uploads) && System.IO.File.Exists(ruta))
            System.IO.File.Delete(ruta);
    }
}
