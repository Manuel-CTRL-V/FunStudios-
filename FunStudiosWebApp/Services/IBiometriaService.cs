using FunStudiosWebApp.Models;

namespace FunStudiosWebApp.Services;

/// <summary>Estado en vivo del lector biométrico remoto.</summary>
public class EstadoBiometria
{
    public bool LectorConectado { get; set; }
    public string NombreDispositivo { get; set; } = "";
    public bool Ocupado { get; set; }
}

/// <summary>
/// Abstracción del lector biométrico DigitalPersona que vive en la PC de recepción
/// y se consume por red a través de la API local de <c>BiometricService</c>.
///
/// El lector es un recurso físico compartido y las operaciones son stateless sobre
/// el hardware: cada llamada captura la huella EN el lector y devuelve el resultado.
///
/// EstadoAsync        → consulta en vivo el estado del lector (conectado, nombre, ocupado).
/// EnrolarAsync       → captura N muestras en el lector y devuelve el template (Base64).
/// IdentificarAsync(1:N) → captura un dedo y verifica contra la BD/templates; devuelve índice.
/// VerificarAsync(1:1) → captura un dedo y compara contra un template específico.
/// </summary>
public interface IBiometriaService
{
    /// <summary>Consulta en vivo el estado del lector remoto.</summary>
    Task<EstadoBiometria> EstadoAsync(CancellationToken ct = default);

    /// <summary>
    /// Enrola una huella capturando <paramref name="muestras"/> muestras en el lector.
    /// Devuelve el template en Base64 para guardarlo, o null si falló.
    /// </summary>
    Task<string?> EnrolarAsync(int muestras, CancellationToken ct = default);

    /// <summary>
    /// Captura un dedo en el lector y lo identifica (1:N) contra la lista de templates
    /// (las de la base de datos). Devuelve el índice de la coincidencia, o -1 si no hay.
    /// </summary>
    Task<int> IdentificarAsync(IReadOnlyList<string> templatesBase64, CancellationToken ct = default);

    /// <summary>
    /// Captura un dedo en el lector y lo verifica (1:1) contra un template específico.
    /// Devuelve true si coincide.
    /// </summary>
    Task<bool> VerificarAsync(string templateBase64, CancellationToken ct = default);

    /// <summary>Escribe un evento en Logs_Biometrico (errores, enrolamientos, identificaciones).</summary>
    Task RegistrarEventoAsync(string operacion, string mensaje, string gravedad,
        int? usuarioId = null, int? clienteId = null, string ip = "");
}