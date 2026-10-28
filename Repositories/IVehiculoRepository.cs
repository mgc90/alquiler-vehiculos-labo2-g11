using alquiler_vehiculos_g11.Models;
using alquiler_vehiculos_g11.Models.Enums;

namespace alquiler_vehiculos_g11.Repositories;

public interface IVehiculoRepository : IWriteRepository<Vehiculo>
{
    Task<IReadOnlyList<CategoriaVehiculo>> ObtenerCategoriasActivasAsync();

    Task<ResultadoPaginado<Vehiculo>> BuscarDisponiblesAsync(
        DateTime entregaPrevista,
        DateTime devolucionPrevista,
        int? categoriaVehiculoId,
        int pagina,
        int tamanioPagina);

    Task<ResultadoPaginado<Vehiculo>> BuscarAsync(
        string termino,
        int pagina,
        int tamanioPagina);

    Task<ResultadoPaginado<Vehiculo>> ObtenerPorEstadoAsync(
        EstadoOperativoVehiculo? estado,
        int pagina,
        int tamanioPagina);

    Task<ResultadoPaginado<Vehiculo>> ObtenerPorCategoriaAsync(
        int? categoriaVehiculoId,
        int pagina,
        int tamanioPagina);

    Task<ResultadoPaginado<VehiculoPopular>> ObtenerMasAlquiladosAsync(
        DateTime desde,
        DateTime hasta,
        int pagina,
        int tamanioPagina);

    Task<ResultadoPaginado<Vehiculo>> ObtenerSinAlquileresDesdeAsync(
        DateTime desde,
        int pagina,
        int tamanioPagina);
}
