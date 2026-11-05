using alquiler_vehiculos_g11.Data;
using alquiler_vehiculos_g11.Models;
using alquiler_vehiculos_g11.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace alquiler_vehiculos_g11.Repositories;

public class ImagenRepository : IImagenRepository
{
    private readonly ApplicationDbContext _context;

    public ImagenRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Imagen>> ObtenerTodosAsync() =>
        await _context.Imagenes
            .AsNoTracking()
            .Include(i => i.Vehiculo)
            .Include(i => i.Alquiler)
            .OrderByDescending(i => i.Id)
            .ToListAsync();

    public async Task<Imagen?> ObtenerPorIdAsync(int id) =>
        await _context.Imagenes
            .Include(i => i.Vehiculo)
            .FirstOrDefaultAsync(i => i.Id == id);

    public async Task<Dictionary<int, string>> ObtenerPortadasPorVehiculosAsync(IEnumerable<int> vehiculoIds)
    {
        var ids = vehiculoIds.ToList();

        var imagenes = await _context.Imagenes
            .Where(i => ids.Contains(i.VehiculoId) && i.AlquilerId == null) // portada: no está ligada a un alquiler
            .OrderBy(i => i.Id)
            .Select(i => new { i.VehiculoId, i.Url })
            .ToListAsync();

        // Si un vehículo tuviera más de una, me quedo con la primera
        return imagenes
            .GroupBy(i => i.VehiculoId)
            .ToDictionary(g => g.Key, g => g.First().Url);
    }

    public async Task<Imagen?> ObtenerPortadaPorVehiculoAsync(int vehiculoId) => 
        await _context.Imagenes
            .Where(i => i.VehiculoId == vehiculoId && i.AlquilerId == null)
            .OrderBy(i => i.Id)
            .FirstOrDefaultAsync();


    public async Task AgregarAsync(Imagen entity)
    {
        _context.Imagenes.Add(entity);
        await _context.SaveChangesAsync();
    }
        
    public async Task ActualizarAsync(Imagen entity)
    {
         _context.Imagenes.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task EliminarAsync(Imagen entity)
    {
         _context.Imagenes.Remove(entity);
        await _context.SaveChangesAsync();
    }
}