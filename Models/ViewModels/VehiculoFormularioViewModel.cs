using System.ComponentModel.DataAnnotations;

namespace alquiler_vehiculos_g11.Models.ViewModels;

public class VehiculoFormularioViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "La patente es obligatoria.")]
    [RegularExpression(
        @"^([A-Za-z]{3}\d{3}|[A-Za-z]{2}\d{3}[A-Za-z]{2})$",
        ErrorMessage = "La patente debe tener el formato ABC123 o AB123CD.")]
    public string Patente { get; set; } = string.Empty;

    [Required(ErrorMessage = "La marca es obligatoria.")]
    [StringLength(50, ErrorMessage = "La marca no puede superar los 50 caracteres.")]
    public string Marca { get; set; } = string.Empty;

    [Required(ErrorMessage = "El modelo es obligatorio.")]
    [StringLength(50, ErrorMessage = "El modelo no puede superar los 50 caracteres.")]
    public string Modelo { get; set; } = string.Empty;

    [Range(0, 2_000_000, ErrorMessage = "El kilometraje debe estar entre 0 y 2.000.000.")]
    public int KilometrajeActual { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Seleccione una categoría.")]
    public int CategoriaVehiculoId { get; set; }

    [Required(ErrorMessage = "Seleccione el estado operativo.")]
    public string EstadoOperativo { get; set; } = "Habilitado";
}
