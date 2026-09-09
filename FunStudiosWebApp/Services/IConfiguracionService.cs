using FunStudiosWebApp.ViewModels;

namespace FunStudiosWebApp.Services;

/// <summary>
/// Configuración general del sistema (solo Administrador): tipos de acceso,
/// métodos de pago y parámetros clave-valor. Provee también la meta de checks
/// de fidelidad para el resto de los servicios.
/// </summary>
public interface IConfiguracionService
{
    Task<ConfiguracionIndexViewModel> ObtenerResumenAsync();

    /// <summary>Meta de checks de fidelidad (Parámetro "Fidelidad.ChecksParaPremio", default 8).</summary>
    Task<int> ObtenerChecksParaPremioAsync();

    /// <summary>Límite diario de checks de fidelidad (Parámetro "Fidelidad.ChecksPorDia", default 1).</summary>
    Task<int> ObtenerChecksPorDiaAsync();

    Task<TipoAccesoEditarViewModel?> ObtenerTipoAccesoParaEditarAsync(int id);
    Task<(bool Exito, string Mensaje)> GuardarTipoAccesoAsync(TipoAccesoEditarViewModel modelo);

    Task<MetodoPagoFormViewModel?> ObtenerMetodoPagoParaEditarAsync(int id);
    Task<(bool Exito, string Mensaje)> CrearMetodoPagoAsync(string nombre);
    Task<(bool Exito, string Mensaje)> ActualizarMetodoPagoAsync(int id, string nombre);
    Task<(bool Exito, string Mensaje)> EliminarMetodoPagoAsync(int id);

    Task<ParametroEditarViewModel?> ObtenerParametroParaEditarAsync(int id);
    Task<(bool Exito, string Mensaje)> GuardarParametroAsync(ParametroEditarViewModel modelo);
}