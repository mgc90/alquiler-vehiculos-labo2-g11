using alquiler_vehiculos_g11.Models;
using alquiler_vehiculos_g11.Models.Enums;

namespace alquiler_vehiculos_g11.Repositories;

public interface IAlquilerRepository : IWriteRepository<Alquiler>
{
    Task<ResultadoPaginado<Alquiler>> BuscarAsync(
        EstadoAlquiler? estado,
        string? cliente,
        string? vehiculo,
        int pagina,
        int tamanioPagina);

    Task<IReadOnlyList<Alquiler>> ObtenerReservadosPorCategoriaAsync(
        int categoriaId);

    Task<IReadOnlyList<Alquiler>> ObtenerEnCursoPorCategoriaAsync(int categoriaId);

    Task<Alquiler?> ObtenerReservadoParaEntregaAsync(
        int alquilerId);

    Task<Alquiler?> ObtenerEnCursoParaDevolucionAsync(
        int alquilerId);

    Task<Alquiler?> ObtenerDetalleAsync(int alquilerId);

    Task<ResultadoPaginado<Alquiler>> ObtenerVigentesAsync(
        DateTime desde,
        DateTime hasta,
        int pagina,
        int tamanioPagina);

    Task<ResultadoPaginado<Alquiler>> ObtenerQueFinalizanAsync(
        DateTime desde,
        DateTime hasta,
        int pagina,
        int tamanioPagina);
}
