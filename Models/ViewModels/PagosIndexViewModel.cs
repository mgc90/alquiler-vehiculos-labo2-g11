using alquiler_vehiculos_g11.Models.Enums;

namespace alquiler_vehiculos_g11.Models.ViewModels;

public class PagosIndexViewModel
{
    public EstadoPago? Estado { get; set; }

    public int? AlquilerId { get; set; }

    public ResultadoPaginado<Pago> Resultado { get; set; } =
        new(Array.Empty<Pago>(), 0, 1, 10);
}
