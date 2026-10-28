using alquiler_vehiculos_g11.Data;
using alquiler_vehiculos_g11.Models;
using Microsoft.EntityFrameworkCore;

namespace alquiler_vehiculos_g11.Repositories;

public class ClienteInformeRepository : IInformeClientesRepository
{
    private readonly ApplicationDbContext _context;

    public ClienteInformeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<ResultadoPaginado<Cliente>> ObtenerLicenciasPorVencerAsync(
        DateTime desde,
        DateTime hasta,
        int pagina,
        int tamanioPagina)
    {
        if (hasta < desde)
        {
            throw new ArgumentException("La fecha hasta no puede ser anterior a la fecha desde.");
        }

        var consulta = _context.Clientes
            .AsNoTracking()
            .Where(cliente => cliente.VencimientoLicencia >= desde.Date
                && cliente.VencimientoLicencia < hasta.Date.AddDays(1))
            .OrderBy(cliente => cliente.VencimientoLicencia)
            .ThenBy(cliente => cliente.Apellido);

        return Paginacion.CrearResultadoAsync(consulta, pagina, tamanioPagina);
    }
}
