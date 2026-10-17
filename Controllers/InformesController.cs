using Microsoft.AspNetCore.Mvc;

namespace alquiler_vehiculos_g11.Controllers;

public class InformesController : Controller
{
    [HttpGet]
    public IActionResult Index() => View();

    [HttpGet]
    public IActionResult Disponibilidad(DateTime? entregaPrevista, DateTime? devolucionPrevista, int? categoriaVehiculoId) => View();

    [HttpGet]
    public IActionResult AlquileresVigentes(DateTime? desde, DateTime? hasta) => View();

    [HttpGet]
    public IActionResult Vehiculos() => View();
}
