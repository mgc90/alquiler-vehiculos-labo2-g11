namespace alquiler_vehiculos_g11.Models.ViewModels;

public class InformeAlquileresVigentesViewModel
{
    public DateTime? Desde { get; set; }

    public DateTime? Hasta { get; set; }

    public string? Error { get; set; }

    public ResultadoPaginado<Alquiler>? Resultado { get; set; }
}
