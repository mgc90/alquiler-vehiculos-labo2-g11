using alquiler_vehiculos_g11.Models;
using Microsoft.AspNetCore.Mvc;

namespace alquiler_vehiculos_g11.Controllers;

public class UsuariosController : Controller
{
    [HttpGet]
    public IActionResult Index() => View();

    [HttpGet]
    public IActionResult Details(int id) => View();

    [HttpGet]
    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Usuario usuario) => View();

    [HttpGet]
    public IActionResult Edit(int id) => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Usuario usuario) => View();

    [HttpGet]
    public IActionResult Delete(int id) => View();

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id) => View("Delete");
}
