namespace alquiler_vehiculos_g11.Models.ViewModels;

public class InformeMasAlquiladosViewModel
{
    public ResultadoPaginado<VehiculoPopular> Resultado { get; set; } =
        new(Array.Empty<VehiculoPopular>(), 0, 1, 10);
}
