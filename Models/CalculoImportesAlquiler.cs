using alquiler_vehiculos_g11.Models.Enums;

namespace alquiler_vehiculos_g11.Models;

public static class CalculoImportesAlquiler
{
    public const decimal PorcentajeMultaCancelacionPrevia = 0.25m;

    private static decimal Redondear(decimal valor) =>
        Math.Round(valor, 2, MidpointRounding.AwayFromZero);

    public static decimal MultaCancelacionPrevia(decimal importeOriginal) =>
        Redondear(importeOriginal * PorcentajeMultaCancelacionPrevia);

    public static int DiasContratados(
        DateTime entregaPrevista,
        DateTime devolucionPrevista) =>
        Math.Max(1, (devolucionPrevista.Date - entregaPrevista.Date).Days);

    public static int DiasUso(
        DateTime entregaReal,
        DateTime devolucionReal) =>
        Math.Max(1, (devolucionReal.Date - entregaReal.Date).Days);

    public static bool EsDevolucionAnticipada(
        DateTime devolucionPrevista,
        DateTime devolucionReal) =>
        devolucionReal.Date < devolucionPrevista.Date;

    public static int DiasDemora(
        DateTime devolucionPrevista,
        DateTime devolucionReal)
    {
        var limite = devolucionPrevista.Date.AddDays(1);
        var demora = devolucionReal - limite;

        return demora > TimeSpan.Zero
            ? (int)Math.Ceiling(demora.TotalDays)
            : 0;
    }

    public static decimal PorcentajeMultaDiasRestantes(
        int diasContratados,
        int diasUso) =>
        diasUso * 2 < diasContratados ? 0.25m : 0.5m;

    public static decimal MultaDiasRestantes(
        decimal precioAcordado,
        int diasContratados,
        int diasUso)
    {
        var diasRestantes = Math.Max(0, diasContratados - diasUso);
        var porcentaje = PorcentajeMultaDiasRestantes(
            diasContratados,
            diasUso);

        return Redondear(
            precioAcordado * diasRestantes * porcentaje);
    }

    public static bool EsCancelacionPosteriorAlRetiro(Alquiler alquiler) =>
        alquiler.EntregaReal.HasValue
        && alquiler.DevolucionReal.HasValue
        && EsDevolucionAnticipada(
            alquiler.DevolucionPrevista,
            alquiler.DevolucionReal.Value);

    public static decimal ImporteUsoEfectivo(Alquiler alquiler)
    {
        if (!EsCancelacionPosteriorAlRetiro(alquiler))
        {
            return 0m;
        }

        var diasUso = DiasUso(
            alquiler.EntregaReal!.Value,
            alquiler.DevolucionReal!.Value);

        return Redondear(alquiler.PrecioAcordado * diasUso);
    }

    public static decimal MultaCancelacion(Alquiler alquiler)
    {
        if (alquiler.Estado == EstadoAlquiler.Cancelado)
        {
            return alquiler.Multa;
        }

        if (alquiler.Estado == EstadoAlquiler.Finalizado
            && EsCancelacionPosteriorAlRetiro(alquiler))
        {
            var diasContratados = DiasContratados(
                alquiler.EntregaPrevista,
                alquiler.DevolucionPrevista);

            var diasUso = DiasUso(
                alquiler.EntregaReal!.Value,
                alquiler.DevolucionReal!.Value);

            return MultaDiasRestantes(
                alquiler.PrecioAcordado,
                diasContratados,
                diasUso);
        }

        return 0m;
    }

    public static decimal OtrosCargos(Alquiler alquiler) =>
        Math.Max(0m, alquiler.Multa - MultaCancelacion(alquiler));

    public static decimal TotalExigible(Alquiler alquiler)
    {
        if (alquiler.Estado == EstadoAlquiler.Cancelado)
        {
            return alquiler.Multa;
        }

        if (alquiler.Estado == EstadoAlquiler.Finalizado
            && EsCancelacionPosteriorAlRetiro(alquiler))
        {
            return ImporteUsoEfectivo(alquiler) + alquiler.Multa;
        }

        return alquiler.ImporteTotal + alquiler.Multa;
    }

    public static decimal PagosVigentes(IEnumerable<Pago> pagos) =>
        pagos
            .Where(pago => pago.Estado == EstadoPago.Vigente)
            .Sum(pago => pago.Importe);

    public static decimal SaldoPendiente(Alquiler alquiler) =>
        TotalExigible(alquiler) - PagosVigentes(alquiler.Pagos);
}
