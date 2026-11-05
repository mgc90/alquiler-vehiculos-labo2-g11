namespace alquiler_vehiculos_g11.Models.Api;

/// <summary>
/// Datos de estado y seguimiento de un alquiler.
/// </summary>
public class AlquilerEstadoResponse
{
    /// <summary>
    /// Identificador del alquiler.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Estado actual del alquiler: Reservado, EnCurso, Finalizado o Cancelado.
    /// </summary>
    public string Estado { get; set; } = string.Empty;

    /// <summary>
    /// Fecha prevista de entrega del vehículo.
    /// </summary>
    public DateTime EntregaPrevista { get; set; }

    /// <summary>
    /// Fecha prevista de devolución del vehículo.
    /// </summary>
    public DateTime DevolucionPrevista { get; set; }

    /// <summary>
    /// Fecha real de entrega, si el vehículo ya fue entregado.
    /// </summary>
    public DateTime? EntregaReal { get; set; }

    /// <summary>
    /// Fecha real de devolución, si el vehículo ya fue devuelto.
    /// </summary>
    public DateTime? DevolucionReal { get; set; }
}
