namespace alquiler_vehiculos_g11.Models.ViewModels;

public class InformeLicenciasViewModel
{
    public int Dias { get; set; } = 30;

    public string? Error { get; set; }

    public ResultadoPaginado<Cliente>? Resultado { get; set; }
}
