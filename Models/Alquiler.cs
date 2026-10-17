using alquiler_vehiculos_g11.Models.Enums;

namespace alquiler_vehiculos_g11.Models;

public class Alquiler
{
    public int Id { get; set; }

    public int ClienteId { get; set; }

    public Cliente? Cliente { get; set; }

    public int VehiculoId { get; set; }

    public Vehiculo? Vehiculo { get; set; }

    public int CreadoPorUsuarioId { get; set; }

    public Usuario? CreadoPorUsuario { get; set; }

    public int? CerradoPorUsuarioId { get; set; }

    public Usuario? CerradoPorUsuario { get; set; }

    public DateTime EntregaPrevista { get; set; }

    public DateTime DevolucionPrevista { get; set; }

    public DateTime? EntregaReal { get; set; }

    public DateTime? DevolucionReal { get; set; }

    public decimal PrecioAcordado { get; set; }

    public string? Observaciones { get; set; }

    public decimal Multa { get; set; }

    public decimal ImporteTotal { get; set; }

    public EstadoAlquiler Estado { get; set; } = EstadoAlquiler.Reservado;

    public ICollection<Pago> Pagos { get; set; } = new List<Pago>();

    public ICollection<Imagen> Imagenes { get; set; } = new List<Imagen>();
}
