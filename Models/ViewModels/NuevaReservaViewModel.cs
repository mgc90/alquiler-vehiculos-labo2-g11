using System.ComponentModel.DataAnnotations;

namespace alquiler_vehiculos_g11.Models.ViewModels;

public class NuevaReservaViewModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un cliente.")]
    public int ClienteId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un vehículo disponible.")]
    public int VehiculoId { get; set; }

    [DataType(DataType.Date)]
    public DateTime EntregaPrevista { get; set; } = DateTime.Today;

    [DataType(DataType.Date)]
    public DateTime DevolucionPrevista { get; set; } = DateTime.Today.AddDays(1);

    public int? CategoriaVehiculoId { get; set; }

    public IReadOnlyList<CategoriaVehiculo> Categorias { get; set; } = Array.Empty<CategoriaVehiculo>();
}
