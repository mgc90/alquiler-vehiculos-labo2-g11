namespace alquiler_vehiculos_g11.Models.ViewModels;

public class InformeSinAlquileresViewModel
{
    public int Dias { get; set; } = 30;

    public string? Error { get; set; }

    public ResultadoPaginado<Vehiculo>? Resultado { get; set; }
}
