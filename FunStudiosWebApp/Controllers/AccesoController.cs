using System.Net.Http;
using FunStudiosWebApp.Data;
using FunStudiosWebApp.Services;
using FunStudiosWebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FunStudiosWebApp.Controllers;

/// <summary>
/// Endpoint del checkpoint: escaneo biométrico simulado y registro de entradas/salidas.
/// Todas las acciones responden JSON porque el checkpoint opera por AJAX sin recargar la página.
/// </summary>
[Authorize]
public class AccesoController : Controller
{
    private readonly IAccesoService _accesoService;
    private readonly Data.FunStudiosContext _context;
    private readonly IBiometriaService _biometria;

    public AccesoController(IAccesoService accesoService, Data.FunStudiosContext context, IBiometriaService biometria)
    {
        _accesoService = accesoService;
        _context = context;
        _biometria = biometria;
    }

    /// <summary>ID del usuario autenticado tomado de los claims.</summary>
    private int? UsuarioId =>
        int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var id) ? id : null;

    // GET: /Acceso/CandidatosHuella
    [HttpGet]
    public async Task<IActionResult> CandidatosHuella()
    {
        try
        {
            var clientes = await _context.Clientes
                .Include(c => c.Familia)
                    .ThenInclude(f => f!.Tutores)
                .Where(c => c.HuellaNinoTemplate != null)
                .ToListAsync();

            var tutoresConHuella = await _context.Tutores
                .Include(t => t.Familia)
                    .ThenInclude(f => f!.Clientes)
                .Where(t => t.HuellaTemplate != null)
                .ToListAsync();

            var candidatos = new List<CandidatoHuellaViewModel>();

            foreach (var cliente in clientes)
            {
                if (cliente.HuellaNinoTemplate != null)
                    candidatos.Add(new CandidatoHuellaViewModel
                    {
                        ClienteId = cliente.ClienteId,
                        TemplateBase64 = Convert.ToBase64String(cliente.HuellaNinoTemplate)
                    });
            }

            foreach (var tutor in tutoresConHuella)
            {
                if (tutor.HuellaTemplate == null || tutor.Familia == null) continue;
                var familia = tutor.Familia;
                var clienteResolver = familia.Clientes
                    .OrderBy(c => c.FechaNacimiento)
                    .FirstOrDefault(c => c.HuellaNinoTemplate == null)
                    ?? familia.Clientes.OrderBy(c => c.FechaNacimiento).FirstOrDefault();
                if (clienteResolver == null) continue;
                candidatos.Add(new CandidatoHuellaViewModel
                {
                    ClienteId = clienteResolver.ClienteId,
                    TemplateBase64 = Convert.ToBase64String(tutor.HuellaTemplate)
                });
            }

            return Json(new RespuestaCandidatosHuellaViewModel { Exitoso = true, Candidatos = candidatos });
        }
        catch (Exception ex)
        {
            return Json(new RespuestaCandidatosHuellaViewModel { Exitoso = false, Mensaje = ex.Message });
        }
    }

    // POST: /Acceso/IdentificarResultado
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> IdentificarResultado([FromBody] IdentificarResultadoRequest req)
    {
        try
        {
            if (!req.ClienteIdResuelto.HasValue)
            {
                await _biometria.RegistrarEventoAsync("Identificacion", "Huella no reconocida en el checkpoint.",
                    "Advertencia", UsuarioId, ip: HttpContext.Connection.RemoteIpAddress?.ToString() ?? "");
                return Json(new ResultadoEscaneoViewModel { Encontrado = false, Mensaje = "Huella no reconocida. Verifique la inscripción del niño." });
            }

            var cliente = await _context.Clientes
                .Include(c => c.Familia)
                    .ThenInclude(f => f!.Tutores)
                .Include(c => c.FidelidadCuenta)
                .Include(c => c.Membresias)
                .FirstOrDefaultAsync(c => c.ClienteId == req.ClienteIdResuelto.Value);

            if (cliente == null)
                return Json(new ResultadoEscaneoViewModel { Encontrado = false, Mensaje = "Huella no reconocida. Verifique la inscripción del niño." });

            await _biometria.RegistrarEventoAsync("Identificacion",
                $"Identificación exitosa: {cliente.Nombre} ({cliente.Familia!.ApellidosFamilia}).",
                "Info", UsuarioId, cliente.ClienteId, ip: HttpContext.Connection.RemoteIpAddress?.ToString() ?? "");

            return Json(await _accesoService.ObtenerResultadoClienteAsync(cliente));
        }
        catch (Exception ex)
        {
            return Json(new ResultadoEscaneoViewModel { Encontrado = false, Mensaje = ex.Message });
        }
    }

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

