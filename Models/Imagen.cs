namespace alquiler_vehiculos_g11.Models;

public class Imagen
{
    public int Id { get; set; }

    public int VehiculoId { get; set; }

    public Vehiculo? Vehiculo { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public int? AlquilerId { get; set; }

    public Alquiler? Alquiler { get; set; }
}
