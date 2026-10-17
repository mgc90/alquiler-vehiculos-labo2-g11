using alquiler_vehiculos_g11.Models;

namespace alquiler_vehiculos_g11.Services;

public interface IAlquilerService
{
    Task<IReadOnlyList<Vehiculo>> BuscarDisponiblesAsync(DateTime entregaPrevista, DateTime devolucionPrevista, int? categoriaVehiculoId);

    Task<Alquiler> CrearReservaAsync(Alquiler alquiler);

    Task EntregarVehiculoAsync(int alquilerId, int usuarioId);

    Task RegistrarDevolucionAsync(int alquilerId, int usuarioId, DateTime devolucionReal, string? observaciones);

    Task CancelarAsync(int alquilerId, int usuarioId);

    Task<Alquiler> ExtenderAsync(int alquilerId, DateTime entregaPrevista, DateTime devolucionPrevista);
}
