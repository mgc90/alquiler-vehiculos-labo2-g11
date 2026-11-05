using alquiler_vehiculos_g11.Data;
using alquiler_vehiculos_g11.Models;
using Microsoft.EntityFrameworkCore;

namespace alquiler_vehiculos_g11.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly ApplicationDbContext _context;

    public UsuarioRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<ResultadoPaginado<ApplicationUser>> ObtenerPaginadoAsync(
        int pagina,
        int tamanioPagina)
    {
        var consulta = _context.Users
            .AsNoTracking()
            .OrderBy(usuario => usuario.Apellido)
            .ThenBy(usuario => usuario.Nombre);

        return Paginacion.CrearResultadoAsync(consulta, pagina, tamanioPagina);
    }
}
