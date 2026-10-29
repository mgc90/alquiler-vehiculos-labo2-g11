using alquiler_vehiculos_g11.Data;
using alquiler_vehiculos_g11.Models;
using Microsoft.EntityFrameworkCore;

namespace alquiler_vehiculos_g11.Repositories;

public class ClienteRepository : IClienteRepository
{
    private readonly ApplicationDbContext _context;

    public ClienteRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Cliente>> ObtenerTodosAsync() =>
        await _context.Clientes
            .AsNoTracking()
            .OrderBy(cliente => cliente.Apellido)
            .ThenBy(cliente => cliente.Nombre)
            .ToListAsync();

    public Task<Cliente?> ObtenerPorIdAsync(int id) =>
        _context.Clientes
            .AsNoTracking()
            .FirstOrDefaultAsync(cliente => cliente.Id == id);

    public async Task<IEnumerable<Cliente>> ObtenerTodasAsync() =>
        await _context.Clientes
            .AsNoTracking()
            .OrderBy(cliente => cliente.Apellido)
            .ThenBy(cliente => cliente.Nombre)
            .ToListAsync();

    public async Task<IEnumerable<Cliente>> ObtenerActivasAsync() =>
        await _context.Clientes
            .AsNoTracking()
            .Where(cliente => cliente.Activo)
            .OrderBy(cliente => cliente.Apellido)
            .ThenBy(cliente => cliente.Nombre)
            .ToListAsync();

    public async Task AgregarAsync(Cliente entity)
    {
        _context.Clientes.Add(entity);
        await _context.SaveChangesAsync();
    }

    public async Task ActualizarAsync(Cliente entity)
    {
        _context.Clientes.Update(entity);
        await _context.SaveChangesAsync();
    }

    public Task CrearAsync(Cliente cliente) => AgregarAsync(cliente);

    public async Task BajaLogicaAsync(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente is null)
        {
            return;
        }

        cliente.Activo = false;
        await _context.SaveChangesAsync();
    }

    public Task<ResultadoPaginado<Cliente>> ObtenerClientesActivo(
        bool activo,
        int pagina,
        int tamanioPagina)
    {
        var consulta = _context.Clientes
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(cliente => cliente.Activo == activo)
            .OrderBy(cliente => cliente.Apellido)
            .ThenBy(cliente => cliente.Nombre);

        return Paginacion.CrearResultadoAsync(consulta, pagina, tamanioPagina);
    }

    public Task<ResultadoPaginado<Cliente>> BuscarAsync(
        bool activo,
        int clienteId,
        int pagina,
        int tamanioPagina)
    {
        var consulta = _context.Clientes
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(cliente => cliente.Activo == activo && cliente.Id == clienteId);

        return Paginacion.CrearResultadoAsync(consulta, pagina, tamanioPagina);
    }

    public Task<ResultadoPaginado<Cliente>> BuscarPorTerminoAsync(
        string termino,
        int pagina,
        int tamanioPagina)
    {
        var consulta = _context.Clientes.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(termino))
        {
            var filtro = termino.Trim();
            consulta = consulta.Where(cliente =>
                cliente.Dni.Contains(filtro)
                || cliente.Nombre.Contains(filtro)
                || cliente.Apellido.Contains(filtro));
        }

        var ordenada = consulta
            .OrderBy(cliente => cliente.Apellido)
            .ThenBy(cliente => cliente.Nombre);

        return Paginacion.CrearResultadoAsync(ordenada, pagina, tamanioPagina);
    }
}
