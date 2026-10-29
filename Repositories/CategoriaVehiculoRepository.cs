using alquiler_vehiculos_g11.Data;
using alquiler_vehiculos_g11.Models;
using Microsoft.EntityFrameworkCore;

namespace alquiler_vehiculos_g11.Repositories;

public class CategoriaVehiculoRepository : ICategoriaVehiculoRepository
{
    private readonly ApplicationDbContext _context;

    public CategoriaVehiculoRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CategoriaVehiculo>> ObtenerTodosAsync() =>
        await _context.CategoriasVehiculo
            .AsNoTracking()
            .OrderBy(categoria => categoria.Nombre)
            .ToListAsync();

    public Task<CategoriaVehiculo?> ObtenerPorIdAsync(int id) =>
        _context.CategoriasVehiculo
            .AsNoTracking()
            .FirstOrDefaultAsync(categoria => categoria.Id == id);

    public async Task<IEnumerable<CategoriaVehiculo>> ObtenerTodasAsync() =>
        await _context.CategoriasVehiculo
            .AsNoTracking()
            .OrderBy(categoria => categoria.Nombre)
            .ToListAsync();

    public async Task<IEnumerable<CategoriaVehiculo>> ObtenerActivasAsync() =>
        await _context.CategoriasVehiculo
            .AsNoTracking()
            .Where(categoria => categoria.Activa)
            .OrderBy(categoria => categoria.Nombre)
            .ToListAsync();

    public async Task AgregarAsync(CategoriaVehiculo entity)
    {
        _context.CategoriasVehiculo.Add(entity);
        await _context.SaveChangesAsync();
    }

    public async Task ActualizarAsync(CategoriaVehiculo entity)
    {
        _context.CategoriasVehiculo.Update(entity);
        await _context.SaveChangesAsync();
    }

    public Task CrearAsync(CategoriaVehiculo categoria) => AgregarAsync(categoria);

    public async Task BajaLogicaAsync(int id)
    {
        var categoria = await _context.CategoriasVehiculo.FindAsync(id);
        if (categoria is null)
        {
            return;
        }

        categoria.Activa = false;
        await _context.SaveChangesAsync();
    }
}
