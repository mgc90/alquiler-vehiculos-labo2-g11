namespace alquiler_vehiculos_g11.Models.ViewModels;

public class PagoDetalleViewModel
{
    public Pago Pago { get; set; } = null!;

    public Alquiler? Alquiler { get; set; }

    public decimal PagosVigentes { get; set; }

    public decimal SaldoPendiente { get; set; }
}
