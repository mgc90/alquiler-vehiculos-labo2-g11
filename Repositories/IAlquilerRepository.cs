using alquiler_vehiculos_g11.Models;
using alquiler_vehiculos_g11.Models.Enums;

namespace alquiler_vehiculos_g11.Repositories;

public interface IAlquilerRepository : IWriteRepository<Alquiler>
{
    Task<ResultadoPaginado<Alquiler>> BuscarAsync(
        EstadoAlquiler? estado,
        int? clienteId,
        int? vehiculoId,
        int pagina,
        int tamanioPagina);

    Task<ResultadoPaginado<Alquiler>> ObtenerVigentesAsync(
        DateTime desde,
        DateTime hasta,
        int pagina,
        int tamanioPagina);
}
