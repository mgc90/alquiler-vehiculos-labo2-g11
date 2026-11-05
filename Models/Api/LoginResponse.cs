namespace alquiler_vehiculos_g11.Models.Api;

/// <summary>
/// Respuesta de autenticación con el token JWT y los datos del usuario.
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// Token JWT que debe enviarse en el encabezado Authorization como Bearer.
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Identificador del usuario autenticado.
    /// </summary>
    public int UsuarioId { get; set; }

    /// <summary>
    /// Nombre del usuario.
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Apellido del usuario.
    /// </summary>
    public string Apellido { get; set; } = string.Empty;

    /// <summary>
    /// Correo electrónico del usuario.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Roles actuales del usuario.
    /// </summary>
    public IList<string> Roles { get; set; } = new List<string>();

    /// <summary>
    /// Fecha y hora UTC en que expira el token.
    /// </summary>
    public DateTime ExpiraEn { get; set; }
}