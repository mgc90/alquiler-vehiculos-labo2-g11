
using alquiler_vehiculos_g11.Models;
using alquiler_vehiculos_g11.Models.Enums;
using alquiler_vehiculos_g11.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace alquiler_vehiculos_g11.Api;

/// <summary>
/// Consultas de vehículos disponibles para la API.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(
    AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
    Roles = "Administrador,Empleado"
)]
public class VehiculosController : ControllerBase
{
    private readonly IAlquilerService _alquilerService;

    private const int TamPagina = 10;

    public VehiculosController(IAlquilerService alquilerService)
    {
        _alquilerService = alquilerService;
    }

    /// <summary>
    /// Consulta los vehículos habilitados y disponibles para un rango de fechas.
    /// </summary>
    /// <param name="entregaPrevista">Fecha y hora previstas de entrega.</param>
    /// <param name="devolucionPrevista">Fecha y hora previstas de devolución; debe ser posterior a la entrega.</param>
    /// <param name="categoriaVehiculoId">Identificador opcional de la categoría de vehículo.</param>
    /// <param name="pagina">Número de página, desde 1.</param>
    /// <param name="alquilerExcluirId">Identificador opcional de un alquiler a excluir, usado al extender una reserva.</param>
    /// <response code="200">Consulta correcta; devuelve los vehículos disponibles paginados.</response>
    /// <response code="400">Las fechas, la página o el alquiler a excluir no son válidos.</response>
    /// <response code="401">No se envió un token JWT válido.</response>
    /// <response code="403">El usuario no tiene el rol Administrador o Empleado.</response>
    [HttpGet("disponibles")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Disponibles(
        [FromQuery] DateTime entregaPrevista,
        [FromQuery] DateTime devolucionPrevista,
        [FromQuery] int? categoriaVehiculoId = null,
        [FromQuery] int pagina = 1,
        [FromQuery] int? alquilerExcluirId = null)
    {
        if (entregaPrevista == default || devolucionPrevista == default)
        {
            return BadRequest(new
            {
                Mensaje = "Las fechas de entrega y devolución son obligatorias."
            });
        }

        if (devolucionPrevista <= entregaPrevista)
        {
            return BadRequest(new
            {
                Mensaje = "La fecha de devolución debe ser posterior a la fecha de entrega."
            });
        }

        if (pagina < 1)
        {
            return BadRequest(new
            {
                Mensaje = "El número de página debe ser mayor o igual a 1."
            });
        }

        if (alquilerExcluirId.HasValue)
        {
            var alquilerOriginal = await _alquilerService.ObtenerPorIdAsync(
                alquilerExcluirId.Value);

            if (alquilerOriginal == null
                || (alquilerOriginal.Estado != EstadoAlquiler.Reservado
                    && alquilerOriginal.Estado != EstadoAlquiler.EnCurso)
                || entregaPrevista.Date != alquilerOriginal.DevolucionPrevista.Date)
            {
                return BadRequest(new
                {
                    Mensaje = "El alquiler indicado no puede utilizarse para esta extensión."
                });
            }
        }

        try
        {
            var resultado = await _alquilerService.BuscarDisponiblesAsync(
                entregaPrevista,
                devolucionPrevista,
                categoriaVehiculoId,
                pagina,
                TamPagina,
                alquilerExcluirId);


            return Ok(new
            {
                resultado.TotalRegistros,
                resultado.Pagina,
                resultado.TamanioPagina,

                Elementos = resultado.Elementos.Select(vehiculo => new
                {
                    vehiculo.Id,
                    vehiculo.Patente,
                    vehiculo.Marca,
                    vehiculo.Modelo,
                    Categoria = vehiculo.CategoriaVehiculo?.Nombre,
                    PrecioDiario = vehiculo.CategoriaVehiculo?.PrecioDiario
                })
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                Mensaje = ex.Message
            });
        }
    }
}
