using alquiler_vehiculos_g11.Models.Enums;

namespace alquiler_vehiculos_g11.Models;

public class Usuario
{
    public int Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public string Apellido { get; set; } = string.Empty;

    public string? Avatar { get; set; }

    public RolUsuario Rol { get; set; }

    public bool Activo { get; set; } = true;

    public ICollection<Alquiler> AlquileresCreados { get; set; } = new List<Alquiler>();

    public ICollection<Alquiler> AlquileresCerrados { get; set; } = new List<Alquiler>();

    public ICollection<Pago> PagosCreados { get; set; } = new List<Pago>();

    public ICollection<Pago> PagosAnulados { get; set; } = new List<Pago>();
}
