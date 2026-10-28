using alquiler_vehiculos_g11.Data;
using alquiler_vehiculos_g11.Models;
using alquiler_vehiculos_g11.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace alquiler_vehiculos_g11.Repositories;

public class AlquilerRepository : IAlquilerRepository
{
    private readonly ApplicationDbContext _context;

    public AlquilerRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Alquiler>> ObtenerTodosAsync() =>
        await _context.Alquileres
            .AsNoTracking()
            .OrderByDescending(alquiler => alquiler.EntregaPrevista)
            .ToListAsync();

    public Task<Alquiler?> ObtenerPorIdAsync(int id) =>
        _context.Alquileres
            .IgnoreQueryFilters()
            .Include(alquiler => alquiler.Cliente)
            .Include(alquiler => alquiler.Vehiculo)
                .ThenInclude(vehiculo => vehiculo!.CategoriaVehiculo)
            .FirstOrDefaultAsync(alquiler => alquiler.Id == id);

    public async Task AgregarAsync(Alquiler entity)
    {
        _context.Alquileres.Add(entity);
        await _context.SaveChangesAsync();
    }

    public async Task ActualizarAsync(Alquiler entity)
    {
        _context.Alquileres.Update(entity);
        await _context.SaveChangesAsync();
    }

    public Task<ResultadoPaginado<Alquiler>> BuscarAsync(
        EstadoAlquiler? estado,
        int? clienteId,
        int? vehiculoId,
        int pagina,
        int tamanioPagina)
    {
        var consulta = CrearConsultaBase()
            .Where(alquiler => estado == null || alquiler.Estado == estado)
            .Where(alquiler => clienteId == null || alquiler.ClienteId == clienteId)
            .Where(alquiler => vehiculoId == null || alquiler.VehiculoId == vehiculoId)
            .OrderByDescending(alquiler => alquiler.EntregaPrevista);

        return Paginacion.CrearResultadoAsync(consulta, pagina, tamanioPagina);
    }

    public Task<ResultadoPaginado<Alquiler>> ObtenerVigentesAsync(
        DateTime desde,
        DateTime hasta,
        int pagina,
        int tamanioPagina)
    {
        if (hasta < desde)
        {
            throw new ArgumentException("La fecha hasta no puede ser anterior a la fecha desde.");
        }

        var consulta = CrearConsultaBase()
            .Where(alquiler =>
                (alquiler.Estado == EstadoAlquiler.Reservado || alquiler.Estado == EstadoAlquiler.EnCurso)
                && alquiler.EntregaPrevista < hasta.Date.AddDays(1)
                && alquiler.DevolucionPrevista >= desde.Date)
            .OrderBy(alquiler => alquiler.EntregaPrevista);

        return Paginacion.CrearResultadoAsync(consulta, pagina, tamanioPagina);
    }

    public Task<ResultadoPaginado<Alquiler>> ObtenerQueFinalizanAsync(
        DateTime desde,
        DateTime hasta,
        int pagina,
        int tamanioPagina)
    {
        if (hasta < desde)
        {
            throw new ArgumentException("La fecha hasta no puede ser anterior a la fecha desde.");
        }

        var consulta = CrearConsultaBase()
            .Where(alquiler =>
                (alquiler.Estado == EstadoAlquiler.Reservado || alquiler.Estado == EstadoAlquiler.EnCurso)
                && alquiler.DevolucionPrevista >= desde
                && alquiler.DevolucionPrevista < hasta.Date.AddDays(1))
            .OrderBy(alquiler => alquiler.DevolucionPrevista);

        return Paginacion.CrearResultadoAsync(consulta, pagina, tamanioPagina);
    }

    private IQueryable<Alquiler> CrearConsultaBase() =>
        _context.Alquileres
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Include(alquiler => alquiler.Cliente)
            .Include(alquiler => alquiler.Vehiculo)
                .ThenInclude(vehiculo => vehiculo!.CategoriaVehiculo)
            .Include(alquiler => alquiler.CreadoPorUsuario)
            .Include(alquiler => alquiler.CerradoPorUsuario);
}
