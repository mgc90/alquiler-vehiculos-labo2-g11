using alquiler_vehiculos_g11.Models;

namespace alquiler_vehiculos_g11.Repositories;

public interface IVehiculoRepository : IWriteRepository<Vehiculo>
{
    Task<IReadOnlyList<Vehiculo>> BuscarDisponiblesAsync(DateTime entregaPrevista, DateTime devolucionPrevista, int? categoriaVehiculoId);

    Task<IReadOnlyList<Vehiculo>> BuscarAsync(string termino, int pagina, int tamanioPagina);
}
