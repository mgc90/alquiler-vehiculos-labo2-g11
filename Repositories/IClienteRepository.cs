using alquiler_vehiculos_g11.Models;

namespace alquiler_vehiculos_g11.Repositories;

public interface IClienteRepository : IWriteRepository<Cliente>
{
    Task<IReadOnlyList<Cliente>> BuscarAsync(string termino, int pagina, int tamanioPagina);
}
