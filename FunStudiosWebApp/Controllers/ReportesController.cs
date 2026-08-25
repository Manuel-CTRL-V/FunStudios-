using FunStudiosWebApp.Services;
using FunStudiosWebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FunStudiosWebApp.Controllers;

/// <summary>Reportes: bitácora de entradas, ventas, BI y logs biométricos (RF12, RF13).</summary>
[Authorize]
public class ReportesController : Controller
{
    private readonly IReporteService _reporteService;

    public ReportesController(IReporteService reporteService)
    {
        _reporteService = reporteService;
    }

    // GET: /Reportes/Bitacora
    public async Task<IActionResult> Bitacora(DateOnly? fechaInicio, DateOnly? fechaFin,
        int? tipoAcceso, string? busqueda)
    {
        var modelo = await _reporteService.ObtenerBitacoraAsync(fechaInicio, fechaFin, tipoAcceso, busqueda);
        return View(modelo);
    }

    // GET: /Reportes/Ventas
    public async Task<IActionResult> Ventas(DateOnly? fechaInicio, DateOnly? fechaFin)
    {
        var modelo = await _reporteService.ObtenerVentasAsync(fechaInicio, fechaFin);
        return View(modelo);
    }

    // GET: /Reportes/InteligenciaNegocio?dias=14
    public async Task<IActionResult> InteligenciaNegocio(int dias = 14)
    {
        var modelo = await _reporteService.ObtenerInteligenciaNegocioAsync(dias);
        return View(modelo);
    }

    // GET: /Reportes/LogsBiometrico?gravedad=Error
    public async Task<IActionResult> LogsBiometrico(string? gravedad)
    {
        var logs = await _reporteService.ObtenerLogsBiometricoAsync(gravedad);
        return View(new LogsBiometricoViewModel { Logs = logs, GravedadFiltro = gravedad });
    }
}
