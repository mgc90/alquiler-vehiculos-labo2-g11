using alquiler_vehiculos_g11.Models;
using alquiler_vehiculos_g11.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using alquiler_vehiculos_g11.Data;
using alquiler_vehiculos_g11.Models.ViewModels;

namespace alquiler_vehiculos_g11.Controllers;

[Authorize]
public class ClientesController : Controller
{

    private readonly IClienteRepository _repository;
    
    public ClientesController(IClienteRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int pagina = 1)
    {
        var clientes = await _repository.ObtenerClientesActivo(true, pagina, 10);
        return View(clientes);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var clientes = await _repository.ObtenerPorIdAsync(id);
        if (clientes == null)
        {
            return NotFound();
        }

        return View(clientes);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new Cliente());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Cliente cliente)
    {

        Cliente? new_cliente = null;

        if (!ModelState.IsValid)
        {
            return View(cliente);
        }

        try
        {
            new_cliente = new Cliente
            {
                Dni = cliente.Dni,
                Nombre = cliente.Nombre,
                Apellido = cliente.Apellido,
                FechaNacimiento = cliente.FechaNacimiento,
                Telefono = cliente.Telefono,
                Email = cliente.Email,
                NumeroLicencia = cliente.NumeroLicencia,
                VencimientoLicencia = cliente.VencimientoLicencia,
            };

            await _repository.CrearAsync(new_cliente);
            TempData["Mensaje"] = "Cliente registrado con éxito.";
            return RedirectToAction(nameof(Index));
        }
        catch
        {
            ModelState.AddModelError("", "No se pudo guardar el cliente.");
            return View(cliente);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {

        Cliente? cliente = null;

        try
        {
            cliente = await _repository.ObtenerPorIdAsync(id);
            if (cliente is null)
            {
                return NotFound();
            }

            return View(new Cliente
            {
                Id = cliente.Id,
                Dni = cliente.Dni,
                Nombre = cliente.Nombre,
                Apellido = cliente.Apellido,
                FechaNacimiento = cliente.FechaNacimiento,
                Telefono = cliente.Telefono,
                Email = cliente.Email,
                NumeroLicencia = cliente.NumeroLicencia,
                VencimientoLicencia = cliente.VencimientoLicencia,
                Alquileres = cliente.Alquileres
            });
        }
        catch
        {
            ModelState.AddModelError("", "Error al cargar los datos del cliente.");
            return View(cliente);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ClienteFormularioViewModel model)
    {

        Cliente? cliente = null;

        try
        {

            if (!ModelState.IsValid)
            {
                return View(ClienteDesde(model));
            }

            if (id != model.Id)
            {
                return BadRequest();
            }

            cliente = await _repository.ObtenerPorIdAsync(id);
            if (cliente is null)
            {
                return NotFound();
            }

            cliente.Dni = model.Dni;
            cliente.Nombre = model.Nombre;
            cliente.Apellido = model.Apellido;
            cliente.FechaNacimiento = model.FechaNacimiento;
            cliente.Telefono = model.Telefono;
            cliente.Email = model.Email;
            cliente.NumeroLicencia = model.NumeroLicencia;
            cliente.VencimientoLicencia = model.VencimientoLicencia;

            await _repository.ActualizarAsync(cliente);
            TempData["Mensaje"] = "Cliente actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch
        {
            ModelState.AddModelError("", "Error al modificar los datos del cliente.");
            return View(cliente ?? ClienteDesde(model));
        }
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {

        Cliente? cliente = null;

        try
        {
            cliente = await _repository.ObtenerPorIdAsync(id);
            if (cliente is null)
            {
                return NotFound();
            }

            return View(cliente);
        }
        catch
        {
            ModelState.AddModelError("", "Error al cargar los datos del cliente.");
            return View(cliente);
        }
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]

    public async Task<IActionResult> DeleteConfirmed(int id)
    {

        Cliente? cliente = null;

        try
        {
            cliente = await _repository.ObtenerPorIdAsync(id);
            if (cliente is null)
            {
                return NotFound();
            }

            await _repository.BajaLogicaAsync(id);
            TempData["Mensaje"] = "Cliente dado de baja con éxito.";
            return RedirectToAction(nameof(Index));
        }
        catch
        {
            ModelState.AddModelError("", "Error al darle de baja al cliente");
            return View(cliente);
        }
    }

    private static Cliente ClienteDesde(ClienteFormularioViewModel model) => new()
    {
        Id = model.Id,
        Dni = model.Dni,
        Nombre = model.Nombre,
        Apellido = model.Apellido,
        FechaNacimiento = model.FechaNacimiento,
        Telefono = model.Telefono,
        Email = model.Email,
        NumeroLicencia = model.NumeroLicencia,
        VencimientoLicencia = model.VencimientoLicencia
    };
}
