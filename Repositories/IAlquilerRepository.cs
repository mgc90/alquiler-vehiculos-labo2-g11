using alquiler_vehiculos_g11.Models;

namespace alquiler_vehiculos_g11.Repositories;

public interface IAlquilerRepository : IWriteRepository<Alquiler>
{
    Task<IReadOnlyList<Alquiler>> ObtenerVigentesAsync(DateTime desde, DateTime hasta, int pagina, int tamanioPagina);
}
