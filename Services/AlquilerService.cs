using alquiler_vehiculos_g11.Models;
using alquiler_vehiculos_g11.Models.Enums;
using alquiler_vehiculos_g11.Repositories;

namespace alquiler_vehiculos_g11.Services;

public class AlquilerService : IAlquilerService
{
    private readonly IAlquilerRepository _alquilerRepository;
    private readonly IVehiculoRepository _vehiculoRepository;
    private readonly IClienteRepository _clienteRepository;

    public AlquilerService(
        IAlquilerRepository alquilerRepository,
        IVehiculoRepository vehiculoRepository,
        IClienteRepository clienteRepository)
    {
        _alquilerRepository = alquilerRepository;
        _vehiculoRepository = vehiculoRepository;
        _clienteRepository = clienteRepository;
    }

    public Task<ResultadoPaginado<Vehiculo>> BuscarDisponiblesAsync(
        DateTime entregaPrevista,
        DateTime devolucionPrevista,
        int? categoriaVehiculoId,
        int pagina,
        int tamanioPagina) =>
        _vehiculoRepository.BuscarDisponiblesAsync(
            entregaPrevista,
            devolucionPrevista,
            categoriaVehiculoId,
            pagina,
            tamanioPagina);

    public async Task<Alquiler> CrearReservaAsync(
        int clienteId,
        int vehiculoId,
        int usuarioId,
        DateTime entregaPrevista,
        DateTime devolucionPrevista)
    {
        var entrega = entregaPrevista.Date;
        var devolucion = devolucionPrevista.Date;

        if (entrega < DateTime.Today)
        {
            throw new ArgumentException("La fecha de entrega no puede ser anterior a hoy.");
        }

        if (devolucion < entrega)
        {
            throw new ArgumentException("La fecha de devolución no puede ser anterior a la de entrega.");
        }

        var cliente = await _clienteRepository.ObtenerPorIdAsync(clienteId)
            ?? throw new InvalidOperationException("El cliente no existe o está dado de baja.");

        if (CalcularEdad(cliente.FechaNacimiento, devolucion) < 18)
        {
            throw new InvalidOperationException("El cliente debe ser mayor de 18 años a la fecha de devolución.");
        }

        if (cliente.VencimientoLicencia.Date < devolucion)
        {
            throw new InvalidOperationException("La licencia del cliente no estará vigente hasta la fecha de devolución.");
        }

        var vehiculo = await _vehiculoRepository.ObtenerPorIdAsync(vehiculoId)
            ?? throw new InvalidOperationException("El vehículo no existe.");

        if (vehiculo.EstadoOperativo != EstadoOperativoVehiculo.Habilitado)
        {
            throw new InvalidOperationException("El vehículo no está habilitado para alquilar.");
        }

        if (!await _vehiculoRepository.EsDisponibleAsync(vehiculoId, entrega, devolucion))
        {
            throw new InvalidOperationException("El vehículo ya está reservado en esas fechas.");
        }

        var precioDiario = vehiculo.CategoriaVehiculo?.PrecioDiario
            ?? throw new InvalidOperationException("El vehículo no tiene una categoría asignada.");

        var dias = Math.Max(1, (devolucion - entrega).Days);

        var alquiler = new Alquiler
        {
            ClienteId = clienteId,
            VehiculoId = vehiculoId,
            CreadoPorUsuarioId = usuarioId,
            EntregaPrevista = entrega,
            DevolucionPrevista = devolucion,
            PrecioAcordado = precioDiario,
            ImporteTotal = precioDiario * dias,
            Estado = EstadoAlquiler.Reservado
        };

        await _alquilerRepository.AgregarAsync(alquiler);
        return alquiler;
    }

    public Task<IReadOnlyList<Alquiler>> ObtenerReservadosPorCategoriaAsync(
        int categoriaId) =>
        _alquilerRepository.ObtenerReservadosPorCategoriaAsync(categoriaId);

    public Task<IReadOnlyList<Alquiler>> ObtenerEnCursoPorCategoriaAsync(
        int categoriaId) =>
        _alquilerRepository.ObtenerEnCursoPorCategoriaAsync(categoriaId);


    public Task<Alquiler?> ObtenerReservadoParaEntregaAsync(int alquilerId) =>
    _alquilerRepository.ObtenerReservadoParaEntregaAsync(alquilerId);

    public Task<Alquiler?> ObtenerEnCursoParaDevolucionAsync(int alquilerId) =>
    _alquilerRepository.ObtenerEnCursoParaDevolucionAsync(alquilerId);

    public async Task<Alquiler> EntregarVehiculoAsync(
        int alquilerId, 
        DateTime entregaReal)
    {
        var alquiler = await _alquilerRepository.ObtenerPorIdAsync(alquilerId);

        var fechaActual = DateTime.Now;

        if (fechaActual < alquiler.EntregaPrevista)
        {
            throw new InvalidOperationException("No se permite alquileres antes de la fecha de entrega prevista.");
        }

        if (alquiler.Estado != EstadoAlquiler.Reservado)
        {
            throw new InvalidOperationException("Solo se pueden entregar alquileres en estado reservado.");
        }

        if (alquiler.Vehiculo!.EstadoOperativo != EstadoOperativoVehiculo.Habilitado)
        {
            throw new InvalidOperationException("El vehículo no está habilitado para entregar.");
        }

        alquiler.EntregaReal = entregaReal;
        alquiler.Estado = EstadoAlquiler.EnCurso;

        await _alquilerRepository.ActualizarAsync(alquiler);

        return alquiler;
    }

    public async Task<Alquiler> RegistrarDevolucionAsync(
    int alquilerId, int usuarioId, DateTime devolucionReal,
    int kilometrajeDevolucion, decimal costosDanios, string? observaciones)
    {
        var alquiler = await _alquilerRepository.ObtenerPorIdAsync(alquilerId)
            ?? throw new InvalidOperationException("El alquiler no existe.");

        if (alquiler.Estado != EstadoAlquiler.EnCurso)
            throw new InvalidOperationException("Solo se pueden devolver alquileres en curso.");

        if (alquiler.EntregaReal is null || devolucionReal < alquiler.EntregaReal)
            throw new InvalidOperationException("La devolución no puede ser anterior a la entrega.");

        if (costosDanios < 0)
            throw new ArgumentException("Los costos por daños no pueden ser negativos.");

        if (kilometrajeDevolucion < alquiler.Vehiculo!.KilometrajeActual)
            throw new ArgumentException("El kilometraje de devolución no puede ser menor al de salida.");

        
        
        //var diasDemora = CalculoAlquiler.DiasDemora(alquiler, devolucionReal);

        var demora = devolucionReal - alquiler.DevolucionPrevista.Date.AddDays(1);
        var diasDemora = demora > TimeSpan.Zero ? (int)Math.Ceiling(demora.TotalDays) : 0;
        var recargo = diasDemora * alquiler.PrecioAcordado;

        alquiler.DevolucionReal = devolucionReal;     // DevolucionPrevista no se toca
        alquiler.Multa = recargo;
        alquiler.ImporteTotal += recargo + costosDanios;
        alquiler.Observaciones = observaciones;
        alquiler.CerradoPorUsuarioId = usuarioId;
        alquiler.Estado = EstadoAlquiler.Finalizado;
        alquiler.Vehiculo.KilometrajeActual = kilometrajeDevolucion;

        await _alquilerRepository.ActualizarAsync(alquiler);
        return alquiler;
    }

    
    public Task<Alquiler?> ObtenerDetalleAsync(int alquilerId) =>
    _alquilerRepository.ObtenerDetalleAsync(alquilerId);

    public Task CancelarAsync(int alquilerId, int usuarioId) =>
        throw new NotImplementedException("Pendiente: módulo de Cancelación, extensión y pagos.");

    public Task<Alquiler> ExtenderAsync(
        int alquilerId,
        int usuarioId,
        DateTime entregaPrevista,
        DateTime devolucionPrevista) =>
        throw new NotImplementedException("Pendiente: módulo de Cancelación, extensión y pagos.");

    private static int CalcularEdad(DateTime fechaNacimiento, DateTime referencia)
    {
        var edad = referencia.Year - fechaNacimiento.Year;
        if (fechaNacimiento.Date > referencia.AddYears(-edad))
        {
            edad--;
        }

        return edad;
    }
}
