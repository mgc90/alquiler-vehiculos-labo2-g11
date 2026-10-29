using alquiler_vehiculos_g11.Models;

namespace alquiler_vehiculos_g11.Repositories;

public interface IClienteRepository : IWriteRepository<Cliente>
{
    //Obtener todos los clientes
    Task<IEnumerable<Cliente>> ObtenerTodasAsync();

    //Obtener todos los clientes ACTIVOS
    Task<IEnumerable<Cliente>> ObtenerActivasAsync();

    //Crear Cliente
    Task CrearAsync(Cliente cliente);

    //Dar de Baja a Cliente (Cambio de estado)
    Task BajaLogicaAsync(int id);

    //Obtener clientes ACTIVOS con paginado
    Task<ResultadoPaginado<Cliente>> ObtenerClientesActivo(
        bool activo,
        int pagina, 
        int tamanioPagina
    );

    //Buscar cliente con paginado
    Task<ResultadoPaginado<Cliente>> BuscarAsync(
        bool activo, 
        int clienteId, 
        int pagina, 
        int tamanioPagina);

    //Buscar clientes activos por DNI, nombre o apellido con paginado
    Task<ResultadoPaginado<Cliente>> BuscarPorTerminoAsync(
        string termino,
        int pagina,
        int tamanioPagina);

}
