using FunStudiosWebApp.Data;
using FunStudiosWebApp.Services;
using FunStudiosWebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FunStudiosWebApp.Controllers;

/// <summary>
/// Endpoint del checkpoint: búsqueda manual de clientes y registro de entradas/salidas.
/// Todas las acciones responden JSON porque el checkpoint opera por AJAX sin recargar la página.
/// </summary>
[Authorize]
public class AccesoController : Controller
{
    private readonly IAccesoService _accesoService;
    private readonly IClienteService _clienteService;
    private readonly Data.FunStudiosContext _context;

    public AccesoController(IAccesoService accesoService, IClienteService clienteService,
        Data.FunStudiosContext context)
    {
        _accesoService = accesoService;
        _clienteService = clienteService;
        _context = context;
    }

    /// <summary>ID del usuario autenticado tomado de los claims.</summary>
    private int? UsuarioId =>
        int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var id) ? id : null;

    // GET: /Acceso/BuscarClientes?q=...
    [HttpGet]
    public async Task<IActionResult> BuscarClientes(string? q)
    {
        var clientes = await _clienteService.BuscarParaCheckpointAsync(q);
        return Json(clientes);
    }

    // POST: /Acceso/IdentificarResultado
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> IdentificarResultado([FromBody] IdentificarResultadoRequest req)
    {
        try
        {
            if (!req.ClienteIdResuelto.HasValue)
                return Json(new ResultadoEscaneoViewModel { Encontrado = false, Mensaje = "Seleccione un cliente." });

            var cliente = await _context.Clientes
                .Include(c => c.Familia)
                    .ThenInclude(f => f!.Tutores)
                .Include(c => c.FidelidadCuenta)
                .Include(c => c.Membresias)
                .FirstOrDefaultAsync(c => c.ClienteId == req.ClienteIdResuelto.Value);

            if (cliente == null)
                return Json(new ResultadoEscaneoViewModel { Encontrado = false, Mensaje = "Cliente no encontrado." });

            return Json(await _accesoService.ObtenerResultadoClienteAsync(cliente));
        }
        catch (Exception ex)
        {
            return Json(new ResultadoEscaneoViewModel { Encontrado = false, Mensaje = ex.Message });
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
    /// Salida manual desde la bitácora o el perfil del niño: marca la salida y
    /// regresa a la pantalla de origen con un mensaje.
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
