using alquiler_vehiculos_g11.Models;

namespace alquiler_vehiculos_g11.Repositories;

public interface ICategoriaVehiculoRepository : IWriteRepository<CategoriaVehiculo>
{
    Task<IEnumerable<CategoriaVehiculo>> ObtenerTodasAsync();
    Task<IEnumerable<CategoriaVehiculo>> ObtenerActivasAsync();
    Task<ResultadoPaginado<CategoriaVehiculo>> BuscarAsync(
        string termino,
        int pagina,
        int tamanioPagina);
    Task CrearAsync(CategoriaVehiculo categoria);
    Task BajaLogicaAsync(int id);


}
