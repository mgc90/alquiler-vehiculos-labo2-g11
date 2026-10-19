using alquiler_vehiculos_g11.Models.Enums;

namespace alquiler_vehiculos_g11.Models;

public class Pago
{
    public int Id { get; set; }

    public int AlquilerId { get; set; }

    public Alquiler? Alquiler { get; set; }

    public int CreadoPorUsuarioId { get; set; }

    public ApplicationUser? CreadoPorUsuario { get; set; }

    public int? AnuladoPorUsuarioId { get; set; }

    public ApplicationUser? AnuladoPorUsuario { get; set; }

    public DateTime Fecha { get; set; }

    public DateTime? FechaAnulacion { get; set; }

    public decimal Importe { get; set; }

    public TipoPago Tipo { get; set; }

    public string MedioPago { get; set; } = string.Empty;

    public EstadoPago Estado { get; set; } = EstadoPago.Vigente;
}
