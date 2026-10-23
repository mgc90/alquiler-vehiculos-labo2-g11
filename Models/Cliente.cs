using System.ComponentModel.DataAnnotations;
using alquiler_vehiculos_g11.Validations;
namespace alquiler_vehiculos_g11.Models;
using System.ComponentModel.DataAnnotations.Schema;

public class Cliente
{
    [Key]
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Campo obligatorio")]
    [RegularExpression(@"^\d{7,8}$", ErrorMessage = "El DNI debe contener entre 7 y 8 números.")]
    public string Dni { get; set; } = string.Empty;

    [Required(ErrorMessage = "Campo obligatorio")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "Campo obligatorio")]
    public string Apellido { get; set; } = string.Empty;

    [Required(ErrorMessage = "Campo obligatorio")]
    [DataType(DataType.Date)]
    [MayorDeEdad(18, ErrorMessage = "Debes ser mayor de 18 años")]
    public DateTime FechaNacimiento { get; set; }

    [Required(ErrorMessage = "Campo obligatorio")]
    [RegularExpression(@"^[0-9+()\s-]{7,20}$", ErrorMessage = "El teléfono contiene caracteres no válidos.")]
    public string? Telefono { get; set; }

    [Required(ErrorMessage = "Campo obligatorio"), EmailAddress(ErrorMessage = "Ingrese un email válido.")]
    [StringLength(150, ErrorMessage = "El email no puede superar los 150 caracteres.")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Campo obligatorio")]
    public string NumeroLicencia { get; set; } = string.Empty;

    [Required(ErrorMessage = "Campo obligatorio")]
    [DataType(DataType.Date)]
    [LicenciaVigente]
    public DateTime VencimientoLicencia { get; set; }

    public bool Activo { get; set; } = true;


    // EF Core ignora esta propiedad al generar la primer migración 
    // En la siguiente etapa quitar el 'not mapped' y crear la tabla correspondiente.  
    [NotMapped]
    public ICollection<Alquiler> Alquileres { get; set; } = new List<Alquiler>();

}
