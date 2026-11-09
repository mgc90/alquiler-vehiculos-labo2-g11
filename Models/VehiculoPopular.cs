namespace alquiler_vehiculos_g11.Models;

public sealed record VehiculoPopular(
    string Patente,
    string Marca,
    string Modelo,
    string? Categoria,
    int CantidadAlquileres);
