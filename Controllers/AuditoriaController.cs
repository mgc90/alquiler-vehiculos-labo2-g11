using alquiler_vehiculos_g11.Models;
using alquiler_vehiculos_g11.Models.ViewModels;
using alquiler_vehiculos_g11.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace alquiler_vehiculos_g11.Controllers;

[Authorize(Roles = "Administrador")]
public class AuditoriaController : Controller
{
    private const int TamanioPagina = 10;

    private readonly IAuditoriaRepository _auditoriaRepository;
    private readonly UserManager<ApplicationUser> _userManager;

    public AuditoriaController(
        IAuditoriaRepository auditoriaRepository,
        UserManager<ApplicationUser> userManager)
    {
        _auditoriaRepository = auditoriaRepository;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        int? usuarioId,
        int paginaAlquileres = 1,
        int paginaPagos = 1)
    {
        var model = new AuditoriaViewModel
        {
            Usuarios = await _userManager.Users
                .AsNoTracking()
                .OrderBy(usuario => usuario.Apellido)
                .ThenBy(usuario => usuario.Nombre)
                .ToListAsync(),
            UsuarioId = usuarioId,
            Alquileres = await _auditoriaRepository.ObtenerAlquileresAsync(
                usuarioId,
                paginaAlquileres,
                TamanioPagina),
            Pagos = await _auditoriaRepository.ObtenerPagosAsync(
                usuarioId,
                paginaPagos,
                TamanioPagina)
        };

        return View(model);
    }
}
