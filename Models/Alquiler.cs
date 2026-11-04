using System.ComponentModel.DataAnnotations.Schema;
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

    public ApplicationUser? CreadoPorUsuario { get; set; }

    public int? CerradoPorUsuarioId { get; set; }

    public ApplicationUser? CerradoPorUsuario { get; set; }

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

    [NotMapped]
    public bool EsCancelacionPosteriorAlRetiro =>
        CalculoImportesAlquiler.EsCancelacionPosteriorAlRetiro(this);

    [NotMapped]
    public decimal ImporteUsoEfectivo =>
        CalculoImportesAlquiler.ImporteUsoEfectivo(this);

    [NotMapped]
    public decimal MultaCancelacion =>
        CalculoImportesAlquiler.MultaCancelacion(this);

    [NotMapped]
    public decimal OtrosCargos =>
        CalculoImportesAlquiler.OtrosCargos(this);

    [NotMapped]
    public decimal TotalExigible =>
        CalculoImportesAlquiler.TotalExigible(this);

    [NotMapped]
    public decimal PagosVigentes =>
        CalculoImportesAlquiler.PagosVigentes(Pagos);

    [NotMapped]
    public decimal SaldoPendiente => TotalExigible - PagosVigentes;
}
