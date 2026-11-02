
using System.ComponentModel.DataAnnotations;
using alquiler_vehiculos_g11.Models.Enums;

namespace alquiler_vehiculos_g11.Models.ViewModels;

public class PagoFormularioViewModel
{
    public int AlquilerId { get; set; }

    [Required(ErrorMessage = "El importe es obligatorio.")]
    [Range(0.01, (double)decimal.MaxValue,
        ErrorMessage = "El importe debe ser mayor a $0.")]
    public decimal Importe { get; set; }

    [Required(ErrorMessage = "Debe seleccionar un tipo de pago.")]
    public TipoPago? Tipo { get; set; }

    [Required(ErrorMessage = "Debe seleccionar un medio de pago.")]
    public string MedioPago { get; set; } = string.Empty;
}
