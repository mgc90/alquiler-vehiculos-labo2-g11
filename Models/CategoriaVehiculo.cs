namespace alquiler_vehiculos_g11.Models;

public class CategoriaVehiculo
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public decimal PrecioDiario { get; set; }

    public decimal DepositoGarantia { get; set; }

    public bool Activa { get; set; } = true;

    public ICollection<Vehiculo> Vehiculos { get; set; } = new List<Vehiculo>();
}
