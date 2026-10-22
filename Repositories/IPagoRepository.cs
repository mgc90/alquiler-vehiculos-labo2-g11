using alquiler_vehiculos_g11.Models;

namespace alquiler_vehiculos_g11.Repositories;

public interface IPagoRepository : IRepository<Pago>
{
    Task AgregarAsync(Pago pago);

    Task AnularAsync(int pagoId, int usuarioId, DateTime fechaAnulacion);

    Task<ResultadoPaginado<Pago>> ObtenerPorAlquilerAsync(
        int alquilerId,
        int pagina,
        int tamanioPagina);
}
