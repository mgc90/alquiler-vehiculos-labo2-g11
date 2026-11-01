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

    Task<Alquiler> EntregarVehiculoAsync(int alquilerId, DateTime entregaReal);

    Task<IReadOnlyList<Alquiler>> ObtenerReservadosPorCategoriaAsync(int categoriaId);

    Task<IReadOnlyList<Alquiler>> ObtenerEnCursoPorCategoriaAsync(int categoriaId);

    Task<Alquiler?> ObtenerReservadoParaEntregaAsync(int alquilerId);

    Task<Alquiler?> ObtenerEnCursoParaDevolucionAsync(
        int alquilerId);

    Task<Alquiler> RegistrarDevolucionAsync(
        int alquilerId,
        int usuarioId,
        DateTime devolucionReal,
        int kilometrajeDevolucion,
        decimal costosDanios,
        string? observaciones);

    Task<Alquiler?> ObtenerDetalleAsync(int alquilerId);
    Task CancelarAsync(int alquilerId, int usuarioId);

    Task<Alquiler> ExtenderAsync(
        int alquilerId,
        int usuarioId,
        DateTime entregaPrevista,
        DateTime devolucionPrevista);
}
