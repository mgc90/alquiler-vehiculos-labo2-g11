namespace alquiler_vehiculos_g11.Models.ViewModels;

public class InformeVehiculosCategoriaViewModel
{
    public int? CategoriaVehiculoId { get; set; }

    public IReadOnlyList<CategoriaVehiculo> Categorias { get; set; } =
        Array.Empty<CategoriaVehiculo>();

    public ResultadoPaginado<Vehiculo> Resultado { get; set; } =
        new(Array.Empty<Vehiculo>(), 0, 1, 10);
}
