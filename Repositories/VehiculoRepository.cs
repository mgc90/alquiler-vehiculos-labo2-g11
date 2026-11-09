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
        int tamanioPagina,
        int? alquilerExcluirId = null)
    {
        var (inicioPeriodo, finPeriodoExclusivo) = RangoDias(entregaPrevista, devolucionPrevista);
        var consulta = _context.Vehiculos
            .AsNoTracking()
            .Include(vehiculo => vehiculo.CategoriaVehiculo)
            .Where(vehiculo => vehiculo.EstadoOperativo == EstadoOperativoVehiculo.Habilitado)
            .Where(vehiculo => categoriaVehiculoId == null || vehiculo.CategoriaVehiculoId == categoriaVehiculoId)
            .Where(vehiculo => !_context.Alquileres.Any(alquiler =>
                alquiler.VehiculoId == vehiculo.Id
                && (alquiler.Estado == EstadoAlquiler.Reservado || alquiler.Estado == EstadoAlquiler.EnCurso)
                && (alquilerExcluirId == null || alquiler.Id != alquilerExcluirId.Value)
                && alquiler.EntregaPrevista < finPeriodoExclusivo
                && alquiler.DevolucionPrevista >= inicioPeriodo))
            .OrderBy(vehiculo => vehiculo.Patente);

        return Paginacion.CrearResultadoAsync(consulta, pagina, tamanioPagina);
    }

    public Task<bool> EsDisponibleAsync(
        int vehiculoId,
        DateTime entregaPrevista,
        DateTime devolucionPrevista,
        int? alquilerExcluirId = null)
    {
        var (inicioPeriodo, finPeriodoExclusivo) =
            RangoDias(entregaPrevista, devolucionPrevista);
        return _context.Vehiculos
            .AsNoTracking()
            .AnyAsync(vehiculo =>
                vehiculo.Id == vehiculoId
                && vehiculo.EstadoOperativo == EstadoOperativoVehiculo.Habilitado
                && !_context.Alquileres.Any(alquiler =>
                    alquiler.VehiculoId == vehiculo.Id
                    && (alquiler.Estado == EstadoAlquiler.Reservado
                        || alquiler.Estado == EstadoAlquiler.EnCurso)
                    && (alquilerExcluirId == null
                        || alquiler.Id != alquilerExcluirId.Value)
                    && alquiler.EntregaPrevista < finPeriodoExclusivo
                    && alquiler.DevolucionPrevista >= inicioPeriodo));
    }

    private static (DateTime Inicio, DateTime FinExclusivo) RangoDias(
        DateTime entregaPrevista,
        DateTime devolucionPrevista)
    {
        if (devolucionPrevista.Date < entregaPrevista.Date)
        {
            throw new ArgumentException("La devolución prevista no puede ser anterior a la entrega prevista.");
        }

        return (entregaPrevista.Date, devolucionPrevista.Date.AddDays(1));
    }

    public Task<ResultadoPaginado<Vehiculo>> BuscarAsync(
        string termino,
        int pagina,
        int tamanioPagina)
    {
        IQueryable<Vehiculo> consulta = _context.Vehiculos
            .AsNoTracking()
            .Include(vehiculo => vehiculo.CategoriaVehiculo);

        if (!string.IsNullOrWhiteSpace(termino))
        {
            var filtro = termino.Trim();
            consulta = consulta.Where(vehiculo =>
                vehiculo.Patente.Contains(filtro)
                || vehiculo.Marca.Contains(filtro)
                || vehiculo.Modelo.Contains(filtro));
        }

        return Paginacion.CrearResultadoAsync(
            consulta.OrderBy(vehiculo => vehiculo.Patente),
            pagina,
            tamanioPagina);
    }

    public Task<bool> ExistePatenteAsync(string patente, int? excluirId = null) =>
        _context.Vehiculos
            .AsNoTracking()
            .AnyAsync(vehiculo =>
                vehiculo.Patente == patente
                && (excluirId == null || vehiculo.Id != excluirId.Value));

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

        var desdeFecha = desde.Date;
        var hastaFecha = hasta.Date.AddDays(1);

        var conteos = _context.Alquileres
            .Where(alquiler => alquiler.Estado != EstadoAlquiler.Cancelado
                && alquiler.EntregaPrevista >= desdeFecha
                && alquiler.EntregaPrevista < hastaFecha)
            .GroupBy(alquiler => alquiler.VehiculoId)
            .Select(grupo => new { VehiculoId = grupo.Key, Cantidad = grupo.Count() });

        var consulta = _context.Vehiculos
            .AsNoTracking()
            .Join(conteos,
                vehiculo => vehiculo.Id,
                conteo => conteo.VehiculoId,
                (vehiculo, conteo) => new
                {
                    vehiculo.Patente,
                    vehiculo.Marca,
                    vehiculo.Modelo,
                    Categoria = vehiculo.CategoriaVehiculo!.Nombre,
                    conteo.Cantidad
                })
            .OrderByDescending(item => item.Cantidad)
            .ThenBy(item => item.Patente)
            .Select(item => new VehiculoPopular(
                item.Patente,
                item.Marca,
                item.Modelo,
                item.Categoria,
                item.Cantidad));

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
