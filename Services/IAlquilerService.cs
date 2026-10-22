using alquiler_vehiculos_g11.Models;

namespace alquiler_vehiculos_g11.Services;

public interface IAlquilerService
{
    
    Task<ResultadoPaginado<Vehiculo>> BuscarDisponiblesAsync(
        DateTime entregaPrevista,
        DateTime devolucionPrevista,
        int? categoriaVehiculoId,
        int pagina,
        int tamanioPagina);

    Task<Alquiler> CrearReservaAsync(
        int clienteId,
        int vehiculoId,
        int usuarioId,
        DateTime entregaPrevista,
        DateTime devolucionPrevista);

    Task EntregarVehiculoAsync(int alquilerId, int usuarioId);

    Task RegistrarDevolucionAsync(
        int alquilerId,
        int usuarioId,
        DateTime devolucionReal,
        decimal costosDanios,
        string? observaciones);

    Task CancelarAsync(int alquilerId, int usuarioId);

    Task<Alquiler> ExtenderAsync(
        int alquilerId,
        int usuarioId,
        DateTime entregaPrevista,
        DateTime devolucionPrevista);
}
