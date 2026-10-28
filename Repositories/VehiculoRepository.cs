using alquiler_vehiculos_g11.Data;
using alquiler_vehiculos_g11.Models;
using alquiler_vehiculos_g11.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace alquiler_vehiculos_g11.Repositories;

public class VehiculoRepository : IVehiculoRepository
{
    private readonly ApplicationDbContext _context;

    public VehiculoRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Vehiculo>> ObtenerTodosAsync() =>
        await _context.Vehiculos
            .AsNoTracking()
            .Include(vehiculo => vehiculo.CategoriaVehiculo)
            .OrderBy(vehiculo => vehiculo.Patente)
            .ToListAsync();

    public async Task<IReadOnlyList<CategoriaVehiculo>> ObtenerCategoriasActivasAsync() =>
        await _context.CategoriasVehiculo
            .AsNoTracking()
            .Where(categoria => categoria.Activa)
            .OrderBy(categoria => categoria.Nombre)
            .ToListAsync();

    public Task<Vehiculo?> ObtenerPorIdAsync(int id) =>
        _context.Vehiculos
            .Include(vehiculo => vehiculo.CategoriaVehiculo)
            .FirstOrDefaultAsync(vehiculo => vehiculo.Id == id);

    public async Task AgregarAsync(Vehiculo entity)
    {
        _context.Vehiculos.Add(entity);
        await _context.SaveChangesAsync();
    }

    public async Task ActualizarAsync(Vehiculo entity)
    {
        _context.Vehiculos.Update(entity);
        await _context.SaveChangesAsync();
    }

    public Task<ResultadoPaginado<Vehiculo>> BuscarDisponiblesAsync(
        DateTime entregaPrevista,
        DateTime devolucionPrevista,
        int? categoriaVehiculoId,
        int pagina,
        int tamanioPagina)
    {
        if (devolucionPrevista < entregaPrevista)
        {
            throw new ArgumentException("La devolución prevista no puede ser anterior a la entrega prevista.");
        }

        var inicioPeriodo = entregaPrevista.Date;
        var finPeriodoExclusivo = devolucionPrevista.Date.AddDays(1);
        var consulta = _context.Vehiculos
            .AsNoTracking()
            .Include(vehiculo => vehiculo.CategoriaVehiculo)
            .Where(vehiculo => vehiculo.EstadoOperativo == EstadoOperativoVehiculo.Habilitado)
            .Where(vehiculo => categoriaVehiculoId == null || vehiculo.CategoriaVehiculoId == categoriaVehiculoId)
            .Where(vehiculo => !_context.Alquileres.Any(alquiler =>
                alquiler.VehiculoId == vehiculo.Id
                && (alquiler.Estado == EstadoAlquiler.Reservado || alquiler.Estado == EstadoAlquiler.EnCurso)
                && alquiler.EntregaPrevista < finPeriodoExclusivo
                && alquiler.DevolucionPrevista >= inicioPeriodo))
            .OrderBy(vehiculo => vehiculo.Patente);

        return Paginacion.CrearResultadoAsync(consulta, pagina, tamanioPagina);
    }

    public Task<ResultadoPaginado<Vehiculo>> BuscarAsync(
        string termino,
        int pagina,
        int tamanioPagina)
    {
        var consulta = _context.Vehiculos
            .AsNoTracking()
            .Include(vehiculo => vehiculo.CategoriaVehiculo)
            .Where(vehiculo =>
                vehiculo.Patente.Contains(termino)
                || vehiculo.Marca.Contains(termino)
                || vehiculo.Modelo.Contains(termino))
            .OrderBy(vehiculo => vehiculo.Patente);

        return Paginacion.CrearResultadoAsync(consulta, pagina, tamanioPagina);
    }

    public Task<ResultadoPaginado<Vehiculo>> ObtenerPorEstadoAsync(
        EstadoOperativoVehiculo? estado,
        int pagina,
        int tamanioPagina)
    {
        var consulta = _context.Vehiculos
            .AsNoTracking()
            .Include(vehiculo => vehiculo.CategoriaVehiculo)
            .Where(vehiculo => estado == null || vehiculo.EstadoOperativo == estado)
            .OrderBy(vehiculo => vehiculo.Patente);

        return Paginacion.CrearResultadoAsync(consulta, pagina, tamanioPagina);
    }

    public Task<ResultadoPaginado<Vehiculo>> ObtenerPorCategoriaAsync(
        int? categoriaVehiculoId,
        int pagina,
        int tamanioPagina)
    {
        var consulta = _context.Vehiculos
            .AsNoTracking()
            .Include(vehiculo => vehiculo.CategoriaVehiculo)
            .Where(vehiculo => categoriaVehiculoId == null || vehiculo.CategoriaVehiculoId == categoriaVehiculoId)
            .OrderBy(vehiculo => vehiculo.CategoriaVehiculo!.Nombre)
            .ThenBy(vehiculo => vehiculo.Patente);

        return Paginacion.CrearResultadoAsync(consulta, pagina, tamanioPagina);
    }

    public async Task<ResultadoPaginado<VehiculoPopular>> ObtenerMasAlquiladosAsync(
        DateTime desde,
        DateTime hasta,
        int pagina,
        int tamanioPagina)
    {
        if (hasta < desde)
        {
            throw new ArgumentException("La fecha hasta no puede ser anterior a la fecha desde.");
        }

        var consulta = _context.Vehiculos
            .AsNoTracking()
            .Include(vehiculo => vehiculo.CategoriaVehiculo)
            .Select(vehiculo => new VehiculoPopular(
                vehiculo,
                _context.Alquileres.Count(alquiler =>
                    alquiler.VehiculoId == vehiculo.Id
                    && alquiler.Estado != EstadoAlquiler.Cancelado
                    && alquiler.EntregaPrevista >= desde.Date
                    && alquiler.EntregaPrevista < hasta.Date.AddDays(1))))
            .Where(item => item.CantidadAlquileres > 0)
            .OrderByDescending(item => item.CantidadAlquileres)
            .ThenBy(item => item.Vehiculo.Patente);

        return await Paginacion.CrearResultadoAsync(consulta, pagina, tamanioPagina);
    }

    public Task<ResultadoPaginado<Vehiculo>> ObtenerSinAlquileresDesdeAsync(
        DateTime desde,
        int pagina,
        int tamanioPagina)
    {
        var hasta = DateTime.Today.AddDays(1);
        var consulta = _context.Vehiculos
            .AsNoTracking()
            .Include(vehiculo => vehiculo.CategoriaVehiculo)
            .Where(vehiculo => !_context.Alquileres.Any(alquiler =>
                alquiler.VehiculoId == vehiculo.Id
                && alquiler.Estado != EstadoAlquiler.Cancelado
                && alquiler.EntregaPrevista >= desde.Date
                && alquiler.EntregaPrevista < hasta))
            .OrderBy(vehiculo => vehiculo.Patente);

        return Paginacion.CrearResultadoAsync(consulta, pagina, tamanioPagina);
    }
}
