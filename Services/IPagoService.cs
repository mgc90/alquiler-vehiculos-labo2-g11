using alquiler_vehiculos_g11.Models;
using alquiler_vehiculos_g11.Models.Enums;

namespace alquiler_vehiculos_g11.Services;

public interface IPagoService
{
    Task<Pago> RegistrarAsync(
        int alquilerId,
        int usuarioId,
        decimal importe,
        TipoPago tipo,
        string medioPago);

    Task AnularAsync(int pagoId, int usuarioId);
}
