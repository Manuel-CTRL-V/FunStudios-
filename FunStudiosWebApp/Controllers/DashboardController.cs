using FunStudiosWebApp.Services;
using FunStudiosWebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FunStudiosWebApp.Controllers;

/// <summary>Panel de control de recepción: métricas + checkpoint biométrico (RF5, RF13).</summary>
[Authorize]
public class DashboardController : Controller
{
    private readonly IReporteService _reporteService;
    private readonly IBiometriaService _biometria;

    public DashboardController(IReporteService reporteService, IBiometriaService biometria)
    {
        _reporteService = reporteService;
        _biometria = biometria;
    }

    public async Task<IActionResult> Index()
    {
        var modelo = await _reporteService.ObtenerDashboardAsync();
        return View(modelo);
    }

    /// <summary>
    /// Estado en vivo del lector biométrico remoto, consultado por la página mediante
    /// polling para que la interfaz se actualice sola cuando cambia el lector
    /// (conectado/desconectado, ocupado) sin recargar toda la página.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> EstadoLector()
    {
        var estado = await _biometria.EstadoAsync(HttpContext.RequestAborted);
        return Json(new
        {
            estado.LectorConectado,
            estado.NombreDispositivo,
            estado.Ocupado
        });
    }
}
