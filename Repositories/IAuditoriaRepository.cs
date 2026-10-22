using alquiler_vehiculos_g11.Models;

namespace alquiler_vehiculos_g11.Repositories;

public interface IAuditoriaRepository
{
    Task<ResultadoPaginado<Alquiler>> ObtenerAlquileresAsync(
        int? usuarioId,
        int pagina,
        int tamanioPagina);

    Task<ResultadoPaginado<Pago>> ObtenerPagosAsync(
        int? usuarioId,
        int pagina,
        int tamanioPagina);
}
