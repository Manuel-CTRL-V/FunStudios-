using FunStudiosWebApp.Models;

namespace FunStudiosWebApp.Services;

/// <summary>
/// Abstracción del hardware biométrico (RNF3). Hoy la implementa el
/// SimuladorBiometriaService; cuando exista el servicio local de Windows con el
/// SDK de DigitalPersona, se registrará una implementación que consuma esa API
/// sin cambiar el resto de la aplicación.
/// </summary>
public interface IBiometriaService
{
    /// <summary>Nombre del lector reportado por el dispositivo.</summary>
    string NombreDispositivo { get; }

    /// <summary>Indica si hay un lector disponible.</summary>
    bool LectorConectado { get; }

    /// <summary>Captura una muestra de huella y devuelve su template.</summary>
    Task<byte[]> CapturarMuestraAsync();

    /// <summary>Compara una muestra recién capturada contra un template registrado.</summary>
    Task<bool> VerificarAsync(byte[] muestraActual, byte[] templateRegistrado);

    /// <summary>Escribe un evento en Logs_Biometrico (errores, enrolamientos, identificaciones).</summary>
    Task RegistrarEventoAsync(string operacion, string mensaje, string gravedad,
        int? usuarioId = null, int? clienteId = null, string ip = "");
}
