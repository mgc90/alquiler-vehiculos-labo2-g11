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
        int tamanioPagina,
        int? alquilerExcluirId = null) =>
        _vehiculoRepository.BuscarDisponiblesAsync(
            entregaPrevista,
            devolucionPrevista,
            categoriaVehiculoId,
            pagina,
            tamanioPagina,
            alquilerExcluirId);

    public async Task<Alquiler> CrearReservaAsync(
        int clienteId,
        int vehiculoId,
        int usuarioId,
        DateTime entregaPrevista,
        DateTime devolucionPrevista,
        int? alquilerExcluirId = null)
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

        if (!await _vehiculoRepository.EsDisponibleAsync(
            vehiculoId,
            entrega,
            devolucion,
            alquilerExcluirId))
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
        var alquiler = await _alquilerRepository.ObtenerPorIdAsync(alquilerId)
    ?? throw new InvalidOperationException("El alquiler no existe.");

    var vehiculo = alquiler.Vehiculo
        ?? throw new InvalidOperationException("El vehículo asociado al alquiler no existe.");

    var fechaActual = DateTime.Now;

        if (fechaActual < alquiler.EntregaPrevista)
        {
            throw new InvalidOperationException("No se permite alquileres antes de la fecha de entrega prevista.");
        }

        if (alquiler.Estado != EstadoAlquiler.Reservado)
        {
            throw new InvalidOperationException("Solo se pueden entregar alquileres en estado reservado.");
        }

        if (vehiculo.EstadoOperativo != EstadoOperativoVehiculo.Habilitado)
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

        var vehiculo = alquiler.Vehiculo
            ?? throw new InvalidOperationException("El vehículo asociado al alquiler no existe.");

        if (kilometrajeDevolucion < vehiculo.KilometrajeActual)
            throw new ArgumentException("El kilometraje de devolución no puede ser menor al de salida.");

        var diasContratados = CalculoImportesAlquiler.DiasContratados(
            alquiler.EntregaPrevista,
            alquiler.DevolucionPrevista);

        var diasUso = CalculoImportesAlquiler.DiasUso(
            alquiler.EntregaReal.Value,
            devolucionReal);

        var esCancelacionPosteriorAlRetiro =
            CalculoImportesAlquiler.EsDevolucionAnticipada(
                alquiler.DevolucionPrevista,
                devolucionReal);

        decimal multaCancelacion = 0m;
        decimal recargoDemora = 0m;
        decimal importeUsoEfectivo = 0m;
        var detalle = new List<string>();

        if (esCancelacionPosteriorAlRetiro)
        {
            var diasRestantes = Math.Max(0, diasContratados - diasUso);
            var porcentaje =
                CalculoImportesAlquiler.PorcentajeMultaDiasRestantes(
                    diasContratados,
                    diasUso);

            multaCancelacion = CalculoImportesAlquiler.MultaDiasRestantes(
                alquiler.PrecioAcordado,
                diasContratados,
                diasUso);

            importeUsoEfectivo = alquiler.PrecioAcordado * diasUso;

            detalle.Add(
                $"Cancelación después del retiro ({DateTime.Now:dd/MM/yyyy HH:mm}).");
            detalle.Add(
                $"Uso efectivo: {diasUso} día(s) × {alquiler.PrecioAcordado:C2} = {importeUsoEfectivo:C2}.");
            detalle.Add(
                $"Multa por días restantes: {(porcentaje * 100):0}% de {diasRestantes} día(s) × {alquiler.PrecioAcordado:C2} = {multaCancelacion:C2}.");
        }
        else
        {
            var diasDemora = CalculoImportesAlquiler.DiasDemora(
                alquiler.DevolucionPrevista,
                devolucionReal);

            recargoDemora = diasDemora * alquiler.PrecioAcordado;

            detalle.Add(diasDemora > 0
                ? $"Recargo por demora: {diasDemora} día(s) × {alquiler.PrecioAcordado:C2} = {recargoDemora:C2}."
                : "Devolución en la fecha prevista: sin recargo por demora.");
        }

        if (costosDanios > 0)
        {
            detalle.Add($"Costos por daños: {costosDanios:C2}.");
        }

        if (!string.IsNullOrWhiteSpace(observaciones))
        {
            detalle.Add($"Observaciones del empleado: {observaciones.Trim()}.");
        }

        var cargos = multaCancelacion + recargoDemora + costosDanios;

        alquiler.DevolucionReal = devolucionReal;
        alquiler.Multa += cargos;

        // ImporteTotal conserva siempre el importe original del alquiler.
        alquiler.Observaciones = CombinarObservaciones(
            alquiler.Observaciones,
            detalle);
        alquiler.CerradoPorUsuarioId = usuarioId;
        alquiler.Estado = EstadoAlquiler.Finalizado;
        vehiculo.KilometrajeActual = kilometrajeDevolucion;

        await _alquilerRepository.ActualizarAsync(alquiler);
        return alquiler;
    }

    private static string CombinarObservaciones(
        string? anteriores,
        IEnumerable<string> nuevas)
    {
        var partes = new List<string>();

        if (!string.IsNullOrWhiteSpace(anteriores))
        {
            partes.Add(anteriores.Trim());
        }

        partes.AddRange(nuevas);

        return string.Join(" ", partes);
    }

    
    public Task<Alquiler?> ObtenerDetalleAsync(int alquilerId) =>
    _alquilerRepository.ObtenerDetalleAsync(alquilerId);


    public async Task CancelarAsync(int alquilerId, int usuarioId)
    {
        var alquiler = await _alquilerRepository.ObtenerPorIdAsync(alquilerId)
            ?? throw new InvalidOperationException(
                "El alquiler no existe.");

        if (alquiler.Estado != EstadoAlquiler.Reservado)
        {
            throw new InvalidOperationException(
                "Solo se pueden cancelar alquileres que estén reservados.");
        }

        if (alquiler.EntregaReal.HasValue)
        {
            throw new InvalidOperationException(
                "No se puede cancelar directamente un alquiler cuyo vehículo ya fue entregado. Primero debe registrarse su devolución física.");
        }

        var multa = CalculoImportesAlquiler.MultaCancelacionPrevia(
            alquiler.ImporteTotal);

        alquiler.Multa += multa;

        // ImporteTotal conserva siempre el importe original del alquiler.
        alquiler.Observaciones = CombinarObservaciones(
            alquiler.Observaciones,
            new[]
            {
                $"Cancelación antes del retiro ({DateTime.Now:dd/MM/yyyy HH:mm}).",
                $"Multa por cancelación: 25% del importe original ({alquiler.ImporteTotal:C2}) = {multa:C2}."
            });

        alquiler.Estado = EstadoAlquiler.Cancelado;
        alquiler.CerradoPorUsuarioId = usuarioId;

        await _alquilerRepository.ActualizarAsync(alquiler);
    }

    public async Task<Alquiler> ExtenderAsync(
    int alquilerId,
    int vehiculoId,
    int usuarioId,
    DateTime entregaPrevista,
    DateTime devolucionPrevista)
    {
        var alquilerOriginal = await _alquilerRepository.ObtenerPorIdAsync(alquilerId)
            ?? throw new InvalidOperationException("El alquiler no existe.");

        if (alquilerOriginal.Estado != EstadoAlquiler.Reservado &&
            alquilerOriginal.Estado != EstadoAlquiler.EnCurso)
        {
            throw new InvalidOperationException(
                "Solo se pueden extender alquileres reservados o en curso.");
        }

        if (alquilerOriginal.DevolucionPrevista.Date < DateTime.Today)
        {
            throw new InvalidOperationException(
                "El alquiler está vencido: primero registre la devolución física para calcular la demora y los daños, y luego cree una nueva reserva.");
        }

        var nuevaEntrega = alquilerOriginal.DevolucionPrevista.Date;

        if (entregaPrevista.Date != nuevaEntrega)
        {
            throw new ArgumentException(
                "La fecha de entrega de la extensión debe coincidir con la fecha de devolución original.");
        }

        var nuevaDevolucion = devolucionPrevista.Date;

        if (nuevaDevolucion <= nuevaEntrega)
        {
            throw new ArgumentException(
                "La nueva fecha de devolución debe ser posterior a la fecha de devolución original.");
        }

        return await CrearReservaAsync(
            alquilerOriginal.ClienteId,
            vehiculoId,
            usuarioId,
            nuevaEntrega,
            nuevaDevolucion,
            alquilerOriginal.Id);
    }
    private static int CalcularEdad(DateTime fechaNacimiento, DateTime referencia)
    {
        var edad = referencia.Year - fechaNacimiento.Year;
        if (fechaNacimiento.Date > referencia.AddYears(-edad))
        {
            edad--;
        }

        return edad;
    }

    public Task<Alquiler?> ObtenerPorIdAsync(int id)
    {
        return _alquilerRepository.ObtenerPorIdAsync(id);
    }
}
