using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace alquiler_vehiculos_g11.Controllers;

[Authorize(Roles = "Administrador")]
public class AuditoriaController : Controller
{
    [HttpGet]
    public IActionResult Index() => View();
}
