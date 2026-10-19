using alquiler_vehiculos_g11.Models;
using alquiler_vehiculos_g11.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace alquiler_vehiculos_g11.Controllers;

[Authorize]
public class PerfilController : Controller
{
    private static readonly string[] ExtensionesPermitidas = { ".jpg", ".jpeg", ".png", ".webp" };

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _entorno;

    public PerfilController(UserManager<ApplicationUser> userManager, IWebHostEnvironment entorno)
    {
        _userManager = userManager;
        _entorno = entorno;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var usuario = await _userManager.GetUserAsync(User);
        if (usuario is null)
        {
            return RedirectToAction("Login", "Acceso");
        }

        ViewBag.Rol = (await _userManager.GetRolesAsync(usuario)).FirstOrDefault();
        return View(usuario);
    }

    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        var usuario = await _userManager.GetUserAsync(User);
        if (usuario is null)
        {
            return RedirectToAction("Login", "Acceso");
        }

        return View(new PerfilViewModel
        {
            Nombre = usuario.Nombre,
            Apellido = usuario.Apellido,
            Email = usuario.Email ?? string.Empty,
            Avatar = usuario.Avatar
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(PerfilViewModel modelo)
    {
        var usuario = await _userManager.GetUserAsync(User);
        if (usuario is null)
        {
            return RedirectToAction("Login", "Acceso");
        }

        if (modelo.ArchivoAvatar is not null && modelo.ArchivoAvatar.Length > 0)
        {
            var extension = Path.GetExtension(modelo.ArchivoAvatar.FileName).ToLowerInvariant();
            if (!ExtensionesPermitidas.Contains(extension))
            {
                ModelState.AddModelError(nameof(modelo.ArchivoAvatar), "Solo se permiten imágenes JPG, PNG o WEBP.");
            }
        }

        if (!ModelState.IsValid)
        {
            modelo.Avatar = usuario.Avatar;
            return View(modelo);
        }

        if (modelo.ArchivoAvatar is not null && modelo.ArchivoAvatar.Length > 0)
        {
            var carpeta = Path.Combine(_entorno.WebRootPath, "uploads");
            Directory.CreateDirectory(carpeta);

            var nombreArchivo = $"{Guid.NewGuid()}{Path.GetExtension(modelo.ArchivoAvatar.FileName).ToLowerInvariant()}";

            using (var stream = new FileStream(Path.Combine(carpeta, nombreArchivo), FileMode.Create))
            {
                await modelo.ArchivoAvatar.CopyToAsync(stream);
            }

            if (!string.IsNullOrEmpty(usuario.Avatar))
            {
                var anterior = Path.Combine(_entorno.WebRootPath, usuario.Avatar);
                if (System.IO.File.Exists(anterior))
                {
                    System.IO.File.Delete(anterior);
                }
            }

            usuario.Avatar = $"uploads/{nombreArchivo}";
        }

        usuario.Nombre = modelo.Nombre;
        usuario.Apellido = modelo.Apellido;
        usuario.Email = modelo.Email;
        usuario.UserName = modelo.Email;

        var resultado = await _userManager.UpdateAsync(usuario);
        if (!resultado.Succeeded)
        {
            AgregarErrores(resultado);
            modelo.Avatar = usuario.Avatar;
            return View(modelo);
        }

        TempData["Mensaje"] = "Datos personales actualizados.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult CambiarContrasena() => View(new CambiarContrasenaViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarContrasena(CambiarContrasenaViewModel modelo)
    {
        var usuario = await _userManager.GetUserAsync(User);
        if (usuario is null)
        {
            return RedirectToAction("Login", "Acceso");
        }

        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        var resultado = await _userManager.ChangePasswordAsync(usuario, modelo.ContrasenaActual, modelo.NuevaContrasena);
        if (!resultado.Succeeded)
        {
            AgregarErrores(resultado);
            return View(modelo);
        }

        TempData["Mensaje"] = "Contraseña actualizada.";
        return RedirectToAction(nameof(Index));
    }

    private void AgregarErrores(IdentityResult resultado)
    {
        foreach (var error in resultado.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }
}
