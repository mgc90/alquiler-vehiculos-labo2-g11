using alquiler_vehiculos_g11.Models.Enums;

namespace alquiler_vehiculos_g11.Models.ViewModels;

public class AlquileresIndexViewModel
{
    public EstadoAlquiler? Estado { get; set; }

    public int? ClienteId { get; set; }

    public int? VehiculoId { get; set; }

    public ResultadoPaginado<Alquiler> Resultado { get; set; } =
        new(Array.Empty<Alquiler>(), 0, 1, 10);
}
