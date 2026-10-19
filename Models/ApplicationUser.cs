using Microsoft.AspNetCore.Identity;

namespace alquiler_vehiculos_g11.Models;

public class ApplicationUser : IdentityUser<int>
{
    public string Nombre { get; set; } = string.Empty;

    public string Apellido { get; set; } = string.Empty;

    public string? Avatar { get; set; }

    public bool Activo { get; set; } = true;
}
