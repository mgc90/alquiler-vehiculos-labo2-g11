namespace alquiler_vehiculos_g11.Models;

public class Cliente
{
    public int Id { get; set; }

    public string Dni { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public string Apellido { get; set; } = string.Empty;

    public DateTime FechaNacimiento { get; set; }

    public string? Telefono { get; set; }

    public string? Email { get; set; }

    public string NumeroLicencia { get; set; } = string.Empty;

    public DateTime VencimientoLicencia { get; set; }

    public bool Activo { get; set; } = true;

    public ICollection<Alquiler> Alquileres { get; set; } = new List<Alquiler>();
}
