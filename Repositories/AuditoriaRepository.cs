using alquiler_vehiculos_g11.Data;
using alquiler_vehiculos_g11.Models;
using Microsoft.EntityFrameworkCore;

namespace alquiler_vehiculos_g11.Repositories;

public class AuditoriaRepository : IAuditoriaRepository
{
    private readonly ApplicationDbContext _context;

    public AuditoriaRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<ResultadoPaginado<Alquiler>> ObtenerAlquileresAsync(
        int? usuarioId,
        int pagina,
        int tamanioPagina)
    {
        var consulta = _context.Alquileres
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Include(alquiler => alquiler.Cliente)
            .Include(alquiler => alquiler.Vehiculo)
                .ThenInclude(vehiculo => vehiculo!.CategoriaVehiculo)
            .Include(alquiler => alquiler.CreadoPorUsuario)
            .Include(alquiler => alquiler.CerradoPorUsuario)
            .Where(alquiler => usuarioId == null
                || alquiler.CreadoPorUsuarioId == usuarioId
                || alquiler.CerradoPorUsuarioId == usuarioId)
            .OrderByDescending(alquiler => alquiler.EntregaPrevista);

        return Paginacion.CrearResultadoAsync(consulta, pagina, tamanioPagina);
    }

    public Task<ResultadoPaginado<Pago>> ObtenerPagosAsync(
        int? usuarioId,
        int pagina,
        int tamanioPagina)
    {
        var consulta = _context.Pagos
            .AsNoTracking()
            .Include(pago => pago.Alquiler)
            .Include(pago => pago.CreadoPorUsuario)
            .Include(pago => pago.AnuladoPorUsuario)
            .Where(pago => usuarioId == null
                || pago.CreadoPorUsuarioId == usuarioId
                || pago.AnuladoPorUsuarioId == usuarioId)
            .OrderByDescending(pago => pago.Fecha);

        return Paginacion.CrearResultadoAsync(consulta, pagina, tamanioPagina);
    }
}
