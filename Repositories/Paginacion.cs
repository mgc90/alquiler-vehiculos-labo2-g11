using alquiler_vehiculos_g11.Models;
using Microsoft.EntityFrameworkCore;

namespace alquiler_vehiculos_g11.Repositories;

internal static class Paginacion
{
    private const int TamanioMaximoPagina = 100;

    public static (int Pagina, int TamanioPagina) Normalizar(int pagina, int tamanioPagina) =>
        (Math.Max(1, pagina), Math.Clamp(tamanioPagina, 1, TamanioMaximoPagina));

    public static async Task<ResultadoPaginado<T>> CrearResultadoAsync<T>(
        IQueryable<T> consulta,
        int pagina,
        int tamanioPagina)
    {
        var (paginaNormalizada, tamanioNormalizado) = Normalizar(pagina, tamanioPagina);
        var totalRegistros = await consulta.CountAsync();
        var elementos = await consulta
            .Skip((paginaNormalizada - 1) * tamanioNormalizado)
            .Take(tamanioNormalizado)
            .ToListAsync();

        return new ResultadoPaginado<T>(
            elementos,
            totalRegistros,
            paginaNormalizada,
            tamanioNormalizado);
    }
}
