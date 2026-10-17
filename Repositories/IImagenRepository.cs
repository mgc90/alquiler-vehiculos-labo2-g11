using alquiler_vehiculos_g11.Models;

namespace alquiler_vehiculos_g11.Repositories;

public interface IImagenRepository : IWriteRepository<Imagen>
{
    Task<IReadOnlyList<Imagen>> ObtenerPorVehiculoAsync(int vehiculoId);
}
