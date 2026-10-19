using alquiler_vehiculos_g11.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace alquiler_vehiculos_g11.Controllers;

[Authorize]
public class PagosController : Controller
{
    [HttpGet]
    public IActionResult Index() => View();

    [HttpGet]
    public IActionResult Details(int id) => View();

    [HttpGet]
    public IActionResult Registrar(int alquilerId) => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Registrar(int alquilerId, Pago pago) => View();

    [HttpGet]
    public IActionResult Anular(int id) => View();

    [HttpPost, ActionName("Anular")]
    [ValidateAntiForgeryToken]
    public IActionResult AnularConfirmado(int id) => View("Anular");
}
