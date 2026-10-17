using alquiler_vehiculos_g11.Models;
using Microsoft.AspNetCore.Mvc;

namespace alquiler_vehiculos_g11.Controllers;

public class ClientesController : Controller
{
    [HttpGet]
    public IActionResult Index() => View();

    [HttpGet]
    public IActionResult Details(int id) => View();

    [HttpGet]
    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Cliente cliente) => View();

    [HttpGet]
    public IActionResult Edit(int id) => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Cliente cliente) => View();

    [HttpGet]
    public IActionResult Delete(int id) => View();

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id) => View("Delete");
}
