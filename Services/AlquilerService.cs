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

    public Task EntregarVehiculoAsync(int alquilerId, int usuarioId) =>
        throw new NotImplementedException("Pendiente: módulo de Entrega y devolución.");

    public Task RegistrarDevolucionAsync(
        int alquilerId,
        int usuarioId,
        DateTime devolucionReal,
        decimal costosDanios,
        string? observaciones) =>
        throw new NotImplementedException("Pendiente: módulo de Entrega y devolución.");

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
