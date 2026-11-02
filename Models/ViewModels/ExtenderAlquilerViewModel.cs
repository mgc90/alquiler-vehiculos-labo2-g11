using System.ComponentModel.DataAnnotations;
using alquiler_vehiculos_g11.Models.Enums;

namespace alquiler_vehiculos_g11.Models.ViewModels;

public class ExtenderAlquilerViewModel
{
    public int AlquilerId { get; set; }

    // Datos del alquiler original
    public string ClienteNombre { get; set; } = string.Empty;

    public string VehiculoOriginal { get; set; } = string.Empty;

    public EstadoAlquiler EstadoOriginal { get; set; }

    [DataType(DataType.Date)]
    public DateTime EntregaExtension { get; set; }

    // Datos que selecciona el empleado
    [Range(1, int.MaxValue,
        ErrorMessage = "Seleccione un vehículo disponible.")]
    public int VehiculoId { get; set; }

    [Required(ErrorMessage = "Seleccione una fecha de devolución.")]
    [DataType(DataType.Date)]
    public DateTime DevolucionPrevista { get; set; }

    public IReadOnlyList<CategoriaVehiculo> Categorias { get; set; }
        = Array.Empty<CategoriaVehiculo>();
}