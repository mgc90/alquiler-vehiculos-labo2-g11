using alquiler_vehiculos_g11.Models;
using alquiler_vehiculos_g11.Repositories;
using alquiler_vehiculos_g11.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace alquiler_vehiculos_g11.Controllers;

[Authorize(Roles = "Administrador")]
public class CategoriasVehiculoController : Controller
{
    private readonly ICategoriaVehiculoRepository _repository;

    public CategoriasVehiculoController(ICategoriaVehiculoRepository repository)
    {
        _repository = repository;
    }

    // GET: CategoriasVehiculo
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var categorias = await _repository.ObtenerTodasAsync();
        return View(categorias);
    }

    // GET: CategoriasVehiculo/Details/5
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var categoria = await _repository.ObtenerPorIdAsync(id);
        if (categoria == null)
        {
            return NotFound();
        }

        return View(categoria);
    }

    // GET: CategoriasVehiculo/Create
    [HttpGet]
    public IActionResult Create()
    {
        return View(new CategoriaVehiculoFormularioViewModel());
    }

    // POST: CategoriasVehiculo/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoriaVehiculoFormularioViewModel model)
    {
        // Validaciones manuales
        if (model.PrecioDiario <= 0){
            ModelState.AddModelError(nameof(model.PrecioDiario), "El precio diario debe ser mayor a $0.");
        }

        if (model.DepositoGarantia <= 0){
            ModelState.AddModelError(nameof(model.DepositoGarantia), "El depósito de garantía debe ser mayor a $0.");
        }

        if (ModelState.IsValid){
            // Mapeo view model a entity
            var categoria = new CategoriaVehiculo
            {
                Nombre = model.Nombre,
                Descripcion = model.Descripcion,
                PrecioDiario = model.PrecioDiario,
                DepositoGarantia = model.DepositoGarantia,
                Activa = true
            };

            await _repository.CrearAsync(categoria);
            TempData["Mensaje"] = "Categoría creada con éxito.";
            return RedirectToAction(nameof(Index));
        }

        return View(model);
    }

    // GET: CategoriasVehiculo/Edit/5
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var categoria = await _repository.ObtenerPorIdAsync(id);
        if (categoria == null){
            return NotFound();
        }

        // Mapeo entidad a view model
        var model = new CategoriaVehiculoFormularioViewModel
        {
            Id = categoria.Id,
            Nombre = categoria.Nombre,
            Descripcion = categoria.Descripcion,
            PrecioDiario = categoria.PrecioDiario,
            DepositoGarantia = categoria.DepositoGarantia
        };

        return View(model);
    }

    // POST: CategoriasVehiculo/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CategoriaVehiculoFormularioViewModel model)
    {
        if (id != model.Id){
            return BadRequest();
        }

        if (model.PrecioDiario <= 0){
            ModelState.AddModelError(nameof(model.PrecioDiario), "El precio diario debe ser mayor a $0.");
        }

        if (model.DepositoGarantia <= 0){
            ModelState.AddModelError(nameof(model.DepositoGarantia), "El depósito de garantía debe ser mayor a $0.");
        }

        if (ModelState.IsValid){
            var categoria = await _repository.ObtenerPorIdAsync(id);
            if (categoria == null)
            {
                return NotFound();
            }

            // Actualización de campos desde ViewModel
            categoria.Nombre = model.Nombre;
            categoria.Descripcion = model.Descripcion;
            categoria.PrecioDiario = model.PrecioDiario;
            categoria.DepositoGarantia = model.DepositoGarantia;

            await _repository.ActualizarAsync(categoria);
            TempData["Mensaje"] = "Categoría actualizada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        return View(model);
    }

    // GET: CategoriasVehiculo/Delete/5
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var categoria = await _repository.ObtenerPorIdAsync(id);
        if (categoria == null)
        {
            return NotFound();
        }

        return View(categoria);
    }

    // POST: CategoriasVehiculo/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _repository.BajaLogicaAsync(id);
        TempData["Mensaje"] = "Categoría desactivada con éxito.";
        return RedirectToAction(nameof(Index));
    }
}
