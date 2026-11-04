
using alquiler_vehiculos_g11.Models;
using alquiler_vehiculos_g11.Models.Enums;
using alquiler_vehiculos_g11.Repositories;

namespace alquiler_vehiculos_g11.Services;

public class PagoService : IPagoService
{
    private readonly IPagoRepository _pagoRepository;
    private readonly IAlquilerRepository _alquilerRepository;

    public PagoService(
        IPagoRepository pagoRepository,
        IAlquilerRepository alquilerRepository)
    {
        _pagoRepository = pagoRepository;
        _alquilerRepository = alquilerRepository;
    }

    public async Task<Pago> RegistrarAsync(
        int alquilerId,
        int usuarioId,
        decimal importe,
        TipoPago tipo,
        string medioPago)
    {
        if (importe <= 0)
        {
            throw new ArgumentException(
                "El importe del pago debe ser mayor que cero.");
        }

        if (!Enum.IsDefined(tipo))
        {
            throw new ArgumentException("El tipo de pago no es válido.");
        }

        if (string.IsNullOrWhiteSpace(medioPago))
        {
            throw new ArgumentException(
                "Debe indicar un medio de pago.");
        }

        var medioNormalizado = medioPago.Trim();

        if (medioNormalizado.Equals(
                "Efectivo",
                StringComparison.OrdinalIgnoreCase))
        {
            medioNormalizado = "Efectivo";
        }
        else if (medioNormalizado.Equals(
                    "Transferencia",
                    StringComparison.OrdinalIgnoreCase))
        {
            medioNormalizado = "Transferencia";
        }
        else
        {
            throw new ArgumentException(
                "El medio de pago debe ser Efectivo o Transferencia.");
        }

        var alquiler = await _alquilerRepository.ObtenerPorIdAsync(alquilerId)
            ?? throw new InvalidOperationException(
                "El alquiler no existe.");

        if (alquiler.Estado == EstadoAlquiler.Cancelado
            && tipo != TipoPago.Multa)
        {
            throw new InvalidOperationException(
                "Un alquiler cancelado solo admite pagos correspondientes a multas.");
        }

        if (tipo == TipoPago.Seña
            && alquiler.Estado != EstadoAlquiler.Reservado)
        {
            throw new InvalidOperationException(
                "La seña solo puede registrarse al momento de crear la reserva.");
        }

        var totalPagado = await _pagoRepository
            .ObtenerTotalVigentePorAlquilerAsync(alquilerId);

        var saldoPendiente = alquiler.TotalExigible - totalPagado;

        if (saldoPendiente <= 0)
        {
            throw new InvalidOperationException(
                "El alquiler no tiene saldo pendiente.");
        }

        if (importe > saldoPendiente)
        {
            throw new InvalidOperationException(
                $"El importe supera el saldo pendiente de {saldoPendiente:C2}.");
        }

        var cargosPendientes = 0m;

        if (alquiler.Multa > 0)
        {
            var cargosPagados = await _pagoRepository
                .ObtenerTotalVigentePorTiposAsync(
                    alquilerId,
                    TipoPago.Multa,
                    TipoPago.Recargo);

            cargosPendientes = Math.Max(
                0m,
                Math.Min(alquiler.Multa - cargosPagados, saldoPendiente));
        }

        if (cargosPendientes > 0 && importe < cargosPendientes)
        {
            throw new InvalidOperationException(
                $"El importe no puede ser menor que los cargos adicionales pendientes de {cargosPendientes:C2}.");
        }

        if (tipo is TipoPago.Multa or TipoPago.Recargo
            && cargosPendientes <= 0)
        {
            throw new InvalidOperationException(
                "El alquiler no tiene cargos adicionales pendientes.");
        }

        var pago = new Pago
        {
            AlquilerId = alquilerId,
            CreadoPorUsuarioId = usuarioId,
            Fecha = DateTime.Now,
            Importe = importe,
            Tipo = tipo,
            MedioPago = medioNormalizado,
            Estado = EstadoPago.Vigente
        };

        await _pagoRepository.AgregarAsync(pago);

        return pago;
    }

    public Task AnularAsync(int pagoId, int usuarioId)
    {
        return _pagoRepository.AnularAsync(
            pagoId,
            usuarioId,
            DateTime.Now);
    }
}
