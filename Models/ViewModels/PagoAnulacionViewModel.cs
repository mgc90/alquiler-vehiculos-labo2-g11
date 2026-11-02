
using alquiler_vehiculos_g11.Models.Enums;

namespace alquiler_vehiculos_g11.Models.ViewModels;

public class PagoAnulacionViewModel
{
    public int Id { get; set; }

    public int AlquilerId { get; set; }

    public decimal Importe { get; set; }

    public TipoPago Tipo { get; set; }

    public string MedioPago { get; set; } = string.Empty;

    public DateTime Fecha { get; set; }
}
