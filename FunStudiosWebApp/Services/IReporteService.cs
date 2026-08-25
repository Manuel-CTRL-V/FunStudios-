using FunStudiosWebApp.ViewModels;

namespace FunStudiosWebApp.Services;

/// <summary>Dashboards, bitácora de entradas, ventas e inteligencia de negocio (RF12, RF13).</summary>
public interface IReporteService
{
    Task<DashboardViewModel> ObtenerDashboardAsync();

    Task<BitacoraViewModel> ObtenerBitacoraAsync(DateOnly? fechaInicio, DateOnly? fechaFin,
        int? tipoAccesoId, string? busquedaCliente);

    Task<VentasViewModel> ObtenerVentasAsync(DateOnly? fechaInicio, DateOnly? fechaFin);

    /// <summary>Métricas BI de los últimos N días.</summary>
    Task<InteligenciaNegocioViewModel> ObtenerInteligenciaNegocioAsync(int dias);

    Task<List<LogBiometricoItemViewModel>> ObtenerLogsBiometricoAsync(string? gravedad);
}
