using Microsoft.AspNetCore.Mvc;

namespace alquiler_vehiculos_g11.Controllers;

public class AuditoriaController : Controller
{
    [HttpGet]
    public IActionResult Index() => View();
}
