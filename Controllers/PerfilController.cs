using Microsoft.AspNetCore.Mvc;

namespace alquiler_vehiculos_g11.Controllers;

public class PerfilController : Controller
{
    [HttpGet]
    public IActionResult Index() => View();

    [HttpGet]
    public IActionResult Edit() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(string nombre, string apellido, string email) => View();

    [HttpGet]
    public IActionResult CambiarContrasena() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CambiarContrasena(string contrasenaActual, string nuevaContrasena) => View();
}
