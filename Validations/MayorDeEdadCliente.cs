using System.ComponentModel.DataAnnotations;

namespace alquiler_vehiculos_g11.Validations;

public class MayorDeEdad: ValidationAttribute
{
    private readonly int _edadMinima;

    public MayorDeEdad(int edadMinima = 18)
    {
        _edadMinima = edadMinima;
        ErrorMessage = $"Debes ser mayor de {edadMinima} años";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not DateTime fechaNacimiento)
            return ValidationResult.Success;

        var hoy = DateTime.Today;
        var edad = hoy.Year - fechaNacimiento.Year;

        if (fechaNacimiento.Date > hoy.AddYears(-edad))
            edad--;

        return edad >= _edadMinima
            ? ValidationResult.Success
            : new ValidationResult(ErrorMessage);
    }
}