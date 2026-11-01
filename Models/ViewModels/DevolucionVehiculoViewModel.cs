using System.ComponentModel.DataAnnotations;
using alquiler_vehiculos_g11.Models.Enums;

namespace alquiler_vehiculos_g11.Models.ViewModels;

public class DevolucionVehiculoViewModel
{
    public IEnumerable<CategoriaVehiculo> Categorias { get; set; } = new List<CategoriaVehiculo>();
    public int CategoriaVehiculoId { get; set; }
    public int AlquilerId { get; set; }
}