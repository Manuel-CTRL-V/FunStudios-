namespace FunStudiosWebApp.Models;

/// <summary>
/// Bitácora de eventos del sistema biométrico: errores, enrolamientos e identificaciones.
/// UsuarioId y ClienteId pueden ser null porque no todos los eventos están ligados a uno de ellos.
/// </summary>
public class LogBiometrico
{
    public int LogId { get; set; }

    public DateTime FechaHoraEvento { get; set; }

    /// <summary>Valores posibles: Info | Advertencia | Error</summary>
    public string? Gravedad { get; set; }

    /// <summary>Ejemplos: Enrolamiento | Identificacion | ConexionLector</summary>
    public string? Operacion { get; set; }

    public string MensajeDetalle { get; set; } = string.Empty;

    /// <summary>Estación desde donde ocurrió el evento.</summary>
    public string IPEstacionTrabajo { get; set; } = string.Empty;

    public int? UsuarioId { get; set; }

    public int? ClienteId { get; set; }

    // Relaciones

    public Usuario? Usuario { get; set; }

    public Cliente? Cliente { get; set; }
}
