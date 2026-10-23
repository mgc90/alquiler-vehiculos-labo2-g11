using System.ComponentModel.DataAnnotations;

namespace alquiler_vehiculos_g11.Validations;

public class LicenciaVigente : ValidationAttribute
{
    public LicenciaVigente()
    {
        ErrorMessage = "La licencia de conducir está vencida";
    }

    public override bool IsValid(object? value)
    {
        if (value is not DateTime fecha) return false;
        return fecha.Date >= DateTime.Today;
    }
}