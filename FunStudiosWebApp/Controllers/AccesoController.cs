using FunStudiosWebApp.Services;
using FunStudiosWebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FunStudiosWebApp.Controllers;

/// <summary>
/// Endpoint del checkpoint: escaneo biométrico simulado y registro de entradas/salidas.
/// Todas las acciones responden JSON porque el checkpoint opera por AJAX sin recargar la página.
/// </summary>
[Authorize]
public class AccesoController : Controller
{
    private readonly IAccesoService _accesoService;

    public AccesoController(IAccesoService accesoService)
    {
        _accesoService = accesoService;
    }

    /// <summary>ID del usuario autenticado tomado de los claims.</summary>
    private int? UsuarioId =>
        int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var id) ? id : null;

    // POST: /Acceso/Escanear
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Escanear([FromBody] EscanearRequest solicitud)
    {
        var resultado = await _accesoService.EscanearAsync(solicitud ?? new EscanearRequest(),
            UsuarioId, HttpContext.Connection.RemoteIpAddress?.ToString() ?? "");

        return Json(resultado);
    }

    // POST: /Acceso/RegistrarEntrada
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegistrarEntrada([FromBody] RegistrarEntradaRequest solicitud)
    {
        var resultado = await _accesoService.RegistrarEntradaAsync(solicitud,
            UsuarioId, HttpContext.Connection.RemoteIpAddress?.ToString() ?? "");

        return Json(resultado);
    }

    // POST: /Acceso/RegistrarSalida/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegistrarSalida(int id)
    {
        var resultado = await _accesoService.RegistrarSalidaAsync(id,
            HttpContext.Connection.RemoteIpAddress?.ToString() ?? "");

        return Json(resultado);
    }
}

