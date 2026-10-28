using alquiler_vehiculos_g11.Models;

namespace alquiler_vehiculos_g11.Repositories;

public interface IInformeClientesRepository
{
    Task<ResultadoPaginado<Cliente>> ObtenerLicenciasPorVencerAsync(
        DateTime desde,
        DateTime hasta,
        int pagina,
        int tamanioPagina);
}
