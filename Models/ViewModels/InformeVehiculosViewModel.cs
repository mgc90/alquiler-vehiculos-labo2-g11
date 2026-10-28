using alquiler_vehiculos_g11.Models.Enums;

namespace alquiler_vehiculos_g11.Models.ViewModels;

public class InformeVehiculosViewModel
{
    public EstadoOperativoVehiculo? Estado { get; set; }

    public ResultadoPaginado<Vehiculo> Resultado { get; set; } =
        new(Array.Empty<Vehiculo>(), 0, 1, 10);
}
