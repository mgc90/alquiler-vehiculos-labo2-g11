namespace alquiler_vehiculos_g11.Models;

public sealed record ResultadoPaginado<T>(
    IReadOnlyList<T> Elementos,
    int TotalRegistros,
    int Pagina,
    int TamanioPagina);
