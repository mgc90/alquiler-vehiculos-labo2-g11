using alquiler_vehiculos_g11.Models;

namespace alquiler_vehiculos_g11.Repositories;

public interface IAuditoriaRepository
{
    Task<IReadOnlyList<Alquiler>> ObtenerAlquileresAsync(int? usuarioId, int pagina, int tamanioPagina);

    Task<IReadOnlyList<Pago>> ObtenerPagosAsync(int? usuarioId, int pagina, int tamanioPagina);
}
