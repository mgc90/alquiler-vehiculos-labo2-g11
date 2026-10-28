namespace alquiler_vehiculos_g11.Models.ViewModels;

public class AuditoriaViewModel
{
    public IReadOnlyList<ApplicationUser> Usuarios { get; set; } = Array.Empty<ApplicationUser>();

    public int? UsuarioId { get; set; }

    public ResultadoPaginado<Alquiler> Alquileres { get; set; } =
        new(Array.Empty<Alquiler>(), 0, 1, 10);

    public ResultadoPaginado<Pago> Pagos { get; set; } =
        new(Array.Empty<Pago>(), 0, 1, 10);
}
