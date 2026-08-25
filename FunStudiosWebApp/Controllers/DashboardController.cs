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

    public DashboardController(IReporteService reporteService)
    {
        _reporteService = reporteService;
    }

    public async Task<IActionResult> Index()
    {
        var modelo = await _reporteService.ObtenerDashboardAsync();
        return View(modelo);
    }
}
