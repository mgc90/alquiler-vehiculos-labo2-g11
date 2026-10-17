using Microsoft.AspNetCore.Mvc;

namespace alquiler_vehiculos_g11.Controllers;

public class AccesoController : Controller
{
    [HttpGet]
    public IActionResult Index() => View("Login");

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Login(string email, string password) => View();

    [HttpGet]
    public IActionResult Logout() => View();
}
