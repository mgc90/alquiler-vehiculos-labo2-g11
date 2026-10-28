namespace alquiler_vehiculos_g11.Models.ViewModels;

public class InformeQueFinalizanViewModel
{
    public int Dias { get; set; } = 7;

    public string? Error { get; set; }

    public ResultadoPaginado<Alquiler>? Resultado { get; set; }
}
