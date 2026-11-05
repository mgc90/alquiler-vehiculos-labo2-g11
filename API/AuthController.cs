
using alquiler_vehiculos_g11.Models;
using alquiler_vehiculos_g11.Models.Api;
using alquiler_vehiculos_g11.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace alquiler_vehiculos_g11.Api;

/// <summary>
/// Autenticación de usuarios para la API.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
    }

    /// <summary>
    /// Autentica un usuario y devuelve un token JWT con sus datos y roles.
    /// </summary>
    /// <param name="modelo">Credenciales del usuario: email y contraseña.</param>
    /// <response code="200">Autenticación correcta; devuelve el token JWT y los datos del usuario.</response>
    /// <response code="401">Credenciales inválidas o usuario inactivo.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginViewModel modelo)
    {
        var usuario = await _userManager.FindByEmailAsync(modelo.Email);

        if (usuario == null || !usuario.Activo)
        {
            return Unauthorized(new
            {
                mensaje = "Email o contraseña incorrectos."
            });
        }

        var resultado = await _signInManager.CheckPasswordSignInAsync(
            usuario,
            modelo.Password,
            lockoutOnFailure: false);

        if (!resultado.Succeeded)
        {
            return Unauthorized(new
            {
                mensaje = "Email o contraseña incorrectos."
            });
        }

        // 4. Obtener los roles actuales del usuario.
        var roles = await _userManager.GetRolesAsync(usuario);

        // 5. Crear los claims del token.
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Email, usuario.Email!),
            new Claim(ClaimTypes.Name, usuario.UserName ?? usuario.Email!)
        };

        foreach (var rol in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, rol));
        }

        // 6. Obtener la configuración JWT.
        var clave = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Falta configurar Jwt:Key.");

        var emisor = _configuration["Jwt:Issuer"];
        var audiencia = _configuration["Jwt:Audience"];

        var duracion = _configuration.GetValue<int>("Jwt:ExpirationMinutes");

        var expiracion = DateTime.UtcNow.AddMinutes(duracion);

        // 7. Generar el token.
        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(clave));

        var credenciales = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: emisor,
            audience: audiencia,
            claims: claims,
            expires: expiracion,
            signingCredentials: credenciales);

        var tokenGenerado = new JwtSecurityTokenHandler()
            .WriteToken(token);

        // 8. Preparar la respuesta.
        var respuesta = new LoginResponse
        {
            Token = tokenGenerado,
            UsuarioId = usuario.Id,
            Nombre = usuario.Nombre,
            Apellido = usuario.Apellido,
            Email = usuario.Email!,
            Roles = roles.ToList(),
            ExpiraEn = expiracion
        };

        return Ok(respuesta);
    }
}
