
using System.ComponentModel.DataAnnotations;

namespace alquiler_vehiculos_g11.Models.ViewModels;

public class CategoriaVehiculoFormularioViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre de la categoría es obligatorio.")]
    [StringLength(50, ErrorMessage = "El nombre no puede superar los 50 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(250, ErrorMessage = "La descripción no puede superar los 250 caracteres.")]
    public string? Descripcion { get; set; }

    [Required(ErrorMessage = "El precio diario es obligatorio.")]
    [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "El precio diario debe ser mayor a $0.")]
    public decimal PrecioDiario { get; set; }

    [Required(ErrorMessage = "El depósito de garantía es obligatorio.")]
    [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "El depósito de garantía debe ser mayor a $0.")]
    public decimal DepositoGarantia { get; set; }
}
