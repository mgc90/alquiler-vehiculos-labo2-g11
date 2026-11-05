using alquiler_vehiculos_g11.Models.Api;
using alquiler_vehiculos_g11.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace alquiler_vehiculos_g11.Api;

/// <summary>
/// Consultas de seguimiento y estado de los alquileres.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(
    AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
    Roles = "Administrador,Empleado"
)]
public class AlquileresController : ControllerBase
{
    private readonly IAlquilerService _alquilerService;

    public AlquileresController(IAlquilerService alquilerService)
    {
        _alquilerService = alquilerService;
    }

    /// <summary>
    /// Obtiene el estado y las fechas de seguimiento de un alquiler.
    /// </summary>
    /// <param name="id">Identificador del alquiler.</param>
    /// <response code="200">Alquiler encontrado.</response>
    /// <response code="401">No se envió un token JWT válido.</response>
    /// <response code="403">El usuario no tiene el rol Administrador o Empleado.</response>
    /// <response code="404">No existe un alquiler con el identificador indicado.</response>
    [HttpGet("{id:int}/estado")]
    [ProducesResponseType(typeof(AlquilerEstadoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerEstado(int id)
    {
        var alquiler = await _alquilerService.ObtenerPorIdAsync(id);

        if (alquiler is null)
        {
            return NotFound(new
            {
                mensaje = "El alquiler no existe."
            });
        }

        var respuesta = new AlquilerEstadoResponse
        {
            Id = alquiler.Id,
            Estado = alquiler.Estado.ToString(),
            EntregaPrevista = alquiler.EntregaPrevista,
            DevolucionPrevista = alquiler.DevolucionPrevista,
            EntregaReal = alquiler.EntregaReal,
            DevolucionReal = alquiler.DevolucionReal
        };

        return Ok(respuesta);
    }
}
