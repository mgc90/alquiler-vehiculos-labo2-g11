using alquiler_vehiculos_g11.Models.Enums;

namespace alquiler_vehiculos_g11.Models.ViewModels;

public class AlquileresIndexViewModel
{
    public EstadoAlquiler? Estado { get; set; }

    public string? Cliente { get; set; }

    public string? Vehiculo { get; set; }

    public ResultadoPaginado<Alquiler> Resultado { get; set; } =
        new(Array.Empty<Alquiler>(), 0, 1, 10);
}
