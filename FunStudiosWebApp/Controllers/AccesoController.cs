using System.Net.Http;
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
    public async Task<IActionResult> Escanear()
    {
        try
        {
            var resultado = await _accesoService.EscanearAsync(
                UsuarioId, HttpContext.Connection.RemoteIpAddress?.ToString() ?? "");

            return Json(resultado);
        }
        catch (HttpRequestException ex)
        {
            // El lector no respondió a tiempo (408) o la API no está disponible.
            // Se presenta un mensaje claro en lugar de un 500 genérico.
            var status = ex.StatusCode?.ToString();
            var mensaje = status switch
            {
                "RequestTimeout" => "No se capturó la huella a tiempo. Coloque el dedo en el lector y vuelva a intentar.",
                _ => "El servicio biométrico no está disponible. Verifique que esté activo en la PC de recepción."
            };

            return Json(new ResultadoEscaneoViewModel { Encontrado = false, Mensaje = mensaje });
        }
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
            HttpContext.Connection.RemoteIpAddress?.ToString() ?? "", UsuarioId);

        return Json(resultado);
    }

    // POST: /Acceso/MarcarSalida/5?retorno=bitacora
    /// <summary>
    /// Salida manual desde la bitácora o el perfil del niño (sin huella): marca la
    /// salida y regresa a la pantalla de origen con un mensaje.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarcarSalida(int id, string? retorno)
    {
        var resultado = await _accesoService.RegistrarSalidaAsync(id,
            HttpContext.Connection.RemoteIpAddress?.ToString() ?? "", UsuarioId);

        TempData[resultado.Exito ? "Mensaje" : "Error"] = resultado.Mensaje;

        if (retorno == "bitacora")
            return RedirectToAction("Bitacora", "Reportes");

        return RedirectToAction("Detalles", "Clientes", new { id });
    }
}

