using alquiler_vehiculos_g11.Models;
using Microsoft.AspNetCore.Mvc;

namespace alquiler_vehiculos_g11.Controllers;

public class AlquileresController : Controller
{
    [HttpGet]
    public IActionResult Index() => View();

    [HttpGet]
    public IActionResult Details(int id) => View();

    [HttpGet]
    public IActionResult NuevaReserva() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult NuevaReserva(Alquiler alquiler) => View();

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
