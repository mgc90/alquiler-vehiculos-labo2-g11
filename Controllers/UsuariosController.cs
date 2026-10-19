using alquiler_vehiculos_g11.Models;
using alquiler_vehiculos_g11.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace alquiler_vehiculos_g11.Controllers;

[Authorize(Roles = "Administrador")]
public class UsuariosController : Controller
{
    private static readonly string[] Roles = { "Administrador", "Empleado" };

    private readonly UserManager<ApplicationUser> _userManager;

    public UsuariosController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var usuarios = new List<UsuarioListaViewModel>();

        var lista = await _userManager.Users
            .OrderBy(u => u.Apellido)
            .ThenBy(u => u.Nombre)
            .ToListAsync();

        foreach (var usuario in lista)
        {
            usuarios.Add(new UsuarioListaViewModel
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Email = usuario.Email ?? string.Empty,
                Rol = (await _userManager.GetRolesAsync(usuario)).FirstOrDefault() ?? string.Empty,
                Activo = usuario.Activo
            });
        }

        return View(usuarios);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var usuario = await _userManager.FindByIdAsync(id.ToString());
        if (usuario is null)
        {
            return NotFound();
        }

        ViewBag.Rol = (await _userManager.GetRolesAsync(usuario)).FirstOrDefault();
        return View(usuario);
    }

    [HttpGet]
    public IActionResult Create()
    {
        CargarRoles();
        return View(new UsuarioFormularioViewModel { Activo = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UsuarioFormularioViewModel modelo)
    {
        if (string.IsNullOrWhiteSpace(modelo.Contrasena))
        {
            ModelState.AddModelError(nameof(modelo.Contrasena), "La contraseña es obligatoria.");
        }

        if (!ModelState.IsValid)
        {
            CargarRoles();
            return View(modelo);
        }

        var usuario = new ApplicationUser
        {
            UserName = modelo.Email,
            Email = modelo.Email,
            EmailConfirmed = true,
            Nombre = modelo.Nombre,
            Apellido = modelo.Apellido,
            Activo = modelo.Activo
        };

        var resultado = await _userManager.CreateAsync(usuario, modelo.Contrasena!);
        if (!resultado.Succeeded)
        {
            AgregarErrores(resultado);
            CargarRoles();
            return View(modelo);
        }

        await _userManager.AddToRoleAsync(usuario, modelo.Rol);

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var usuario = await _userManager.FindByIdAsync(id.ToString());
        if (usuario is null)
        {
            return NotFound();
        }

        CargarRoles();
        return View(new UsuarioFormularioViewModel
        {
            Id = usuario.Id,
            Nombre = usuario.Nombre,
            Apellido = usuario.Apellido,
            Email = usuario.Email ?? string.Empty,
            Rol = (await _userManager.GetRolesAsync(usuario)).FirstOrDefault() ?? Roles[1],
            Activo = usuario.Activo
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UsuarioFormularioViewModel modelo)
    {
        var usuario = await _userManager.FindByIdAsync(modelo.Id.ToString());
        if (usuario is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            CargarRoles();
            return View(modelo);
        }

        usuario.Nombre = modelo.Nombre;
        usuario.Apellido = modelo.Apellido;
        usuario.Email = modelo.Email;
        usuario.UserName = modelo.Email;
        usuario.Activo = modelo.Activo;

        var resultado = await _userManager.UpdateAsync(usuario);
        if (!resultado.Succeeded)
        {
            AgregarErrores(resultado);
            CargarRoles();
            return View(modelo);
        }

        var rolesActuales = await _userManager.GetRolesAsync(usuario);
        if (!rolesActuales.Contains(modelo.Rol))
        {
            await _userManager.RemoveFromRolesAsync(usuario, rolesActuales);
            await _userManager.AddToRoleAsync(usuario, modelo.Rol);
        }

        if (!string.IsNullOrWhiteSpace(modelo.Contrasena))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
            var resultadoContrasena = await _userManager.ResetPasswordAsync(usuario, token, modelo.Contrasena);

            if (!resultadoContrasena.Succeeded)
            {
                AgregarErrores(resultadoContrasena);
                CargarRoles();
                return View(modelo);
            }
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var usuario = await _userManager.FindByIdAsync(id.ToString());
        if (usuario is null)
        {
            return NotFound();
        }

        ViewBag.Rol = (await _userManager.GetRolesAsync(usuario)).FirstOrDefault();
        return View(usuario);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var usuario = await _userManager.FindByIdAsync(id.ToString());
        if (usuario is null)
        {
            return NotFound();
        }

        usuario.Activo = false;
        await _userManager.UpdateAsync(usuario);

        return RedirectToAction(nameof(Index));
    }

    private void CargarRoles()
    {
        ViewBag.Roles = new SelectList(Roles);
    }

    private void AgregarErrores(IdentityResult resultado)
    {
        foreach (var error in resultado.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }
}
