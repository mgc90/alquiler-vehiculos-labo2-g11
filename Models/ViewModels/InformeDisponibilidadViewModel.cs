namespace alquiler_vehiculos_g11.Models.ViewModels;

public class InformeDisponibilidadViewModel
{
    public DateTime? EntregaPrevista { get; set; }

    public DateTime? DevolucionPrevista { get; set; }

    public int? CategoriaVehiculoId { get; set; }

    public IReadOnlyList<CategoriaVehiculo> Categorias { get; set; } =
        Array.Empty<CategoriaVehiculo>();

    public string? Error { get; set; }

    public ResultadoPaginado<Vehiculo>? Resultado { get; set; }
}
