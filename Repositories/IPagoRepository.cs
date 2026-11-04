using alquiler_vehiculos_g11.Models;
using alquiler_vehiculos_g11.Models.Enums;

namespace alquiler_vehiculos_g11.Repositories;

public interface IPagoRepository : IRepository<Pago>
{
    Task AgregarAsync(Pago pago);

    Task AnularAsync(int pagoId, int usuarioId, DateTime fechaAnulacion);

    Task<decimal> ObtenerTotalVigentePorAlquilerAsync(int alquilerId);

    Task<decimal> ObtenerTotalVigentePorTiposAsync(
        int alquilerId,
        params TipoPago[] tipos);

    Task<ResultadoPaginado<Pago>> ObtenerPorAlquilerAsync(
        int alquilerId,
        int pagina,
        int tamanioPagina);

    Task<ResultadoPaginado<Pago>> BuscarAsync(
        EstadoPago? estado,
        int? alquilerId,
        int pagina,
        int tamanioPagina);
}
