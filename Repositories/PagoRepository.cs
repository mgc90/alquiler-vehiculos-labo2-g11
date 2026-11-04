using alquiler_vehiculos_g11.Data;
using alquiler_vehiculos_g11.Models;
using alquiler_vehiculos_g11.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace alquiler_vehiculos_g11.Repositories;

public class PagoRepository : IPagoRepository
{
    private readonly ApplicationDbContext _context;

    public PagoRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Pago>> ObtenerTodosAsync()
    {
        return await _context.Pagos
            .AsNoTracking()
            .Include(pago => pago.Alquiler)
            .Include(pago => pago.CreadoPorUsuario)
            .Include(pago => pago.AnuladoPorUsuario)
            .OrderByDescending(pago => pago.Fecha)
            .ToListAsync();
    }

    public Task<Pago?> ObtenerPorIdAsync(int id)
    {
        return _context.Pagos
            .Include(pago => pago.Alquiler)
            .Include(pago => pago.CreadoPorUsuario)
            .Include(pago => pago.AnuladoPorUsuario)
            .FirstOrDefaultAsync(pago => pago.Id == id);
    }

    public async Task AgregarAsync(Pago pago)
    {
        _context.Pagos.Add(pago);
        await _context.SaveChangesAsync();
    }

    public async Task AnularAsync(
        int pagoId,
        int usuarioId,
        DateTime fechaAnulacion)
    {
        var pago = await _context.Pagos
            .FirstOrDefaultAsync(pago => pago.Id == pagoId);

        if (pago == null)
        {
            throw new KeyNotFoundException(
                $"No se encontró el pago con ID {pagoId}.");
        }

        if (pago.Estado != EstadoPago.Vigente)
        {
            throw new InvalidOperationException(
                "El pago ya se encuentra anulado.");
        }

        pago.Estado = EstadoPago.Anulado;
        pago.AnuladoPorUsuarioId = usuarioId;
        pago.FechaAnulacion = fechaAnulacion;

        await _context.SaveChangesAsync();
    }

    public Task<ResultadoPaginado<Pago>> ObtenerPorAlquilerAsync(
        int alquilerId,
        int pagina,
        int tamanioPagina)
    {
        var consulta = _context.Pagos
            .AsNoTracking()
            .Where(pago => pago.AlquilerId == alquilerId)
            .Include(pago => pago.CreadoPorUsuario)
            .Include(pago => pago.AnuladoPorUsuario)
            .OrderByDescending(pago => pago.Fecha);

        return Paginacion.CrearResultadoAsync(
            consulta,
            pagina,
            tamanioPagina);
    }

    public async Task<decimal> ObtenerTotalVigentePorAlquilerAsync(int alquilerId)
    {
        return await _context.Pagos
            .Where(pago =>
                pago.AlquilerId == alquilerId &&
                pago.Estado == EstadoPago.Vigente)
            .SumAsync(pago => pago.Importe);
    }

    public async Task<decimal> ObtenerTotalVigentePorTiposAsync(
        int alquilerId,
        params TipoPago[] tipos)
    {
        var consulta = _context.Pagos
            .Where(pago =>
                pago.AlquilerId == alquilerId &&
                pago.Estado == EstadoPago.Vigente);

        if (tipos.Length > 0)
        {
            consulta = consulta.Where(pago => tipos.Contains(pago.Tipo));
        }

        return await consulta.SumAsync(pago => pago.Importe);
    }

    public Task<ResultadoPaginado<Pago>> BuscarAsync(
        EstadoPago? estado,
        int? alquilerId,
        int pagina,
        int tamanioPagina)
    {
        var consulta = _context.Pagos
            .AsNoTracking()
            .Include(pago => pago.Alquiler)
            .Include(pago => pago.CreadoPorUsuario)
            .Include(pago => pago.AnuladoPorUsuario)
            .Where(pago => estado == null || pago.Estado == estado)
            .Where(pago => alquilerId == null || pago.AlquilerId == alquilerId)
            .OrderByDescending(pago => pago.Fecha);

        return Paginacion.CrearResultadoAsync(
            consulta,
            pagina,
            tamanioPagina);
    }
}
