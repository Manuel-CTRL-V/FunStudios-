using FunStudiosWebApp.Services;
using FunStudiosWebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FunStudiosWebApp.Controllers;

/// <summary>Gesti??n de clientes: listado, perfil, registro con enrolamiento y edici??n (RF1 a RF4).</summary>
[Authorize]
public class ClientesController : Controller
{
    private readonly IClienteService _clienteService;
    private readonly IBiometriaService _biometria;

    public ClientesController(IClienteService clienteService, IBiometriaService biometria)
    {
        _clienteService = clienteService;
        _biometria = biometria;
    }

    private int? UsuarioId =>
        int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var id) ? id : null;

    // GET: /Clientes
    public async Task<IActionResult> Index(string? busqueda, string? estado)
    {
        ViewData["Busqueda"] = busqueda;
        ViewData["Estado"] = estado;
        var clientes = await _clienteService.ObtenerListadoAsync(busqueda, estado);
        return View(clientes);
    }

    // GET: /Clientes/Detalles/5
    public async Task<IActionResult> Detalles(int? id)
    {
        if (id == null) return NotFound();

        var cliente = await _clienteService.ObtenerDetalleAsync(id.Value);
        if (cliente == null) return NotFound();

        return View(cliente);
    }

    // GET: /Clientes/Crear
    public async Task<IActionResult> Crear()
    {
        var modelo = new ClienteCrearViewModel
        {
            FamiliasExistentes = await _clienteService.ObtenerFamiliasAsync()
        };
        return View(modelo);
    }

    // POST: /Clientes/Crear
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(ClienteCrearViewModel modelo)
    {
        modelo.FamiliasExistentes = await _clienteService.ObtenerFamiliasAsync();

        var (exito, mensaje, clienteId) = await _clienteService.RegistrarAsync(
            modelo, UsuarioId, HttpContext.Connection.RemoteIpAddress?.ToString() ?? "");

        TempData[exito ? "Mensaje" : "Error"] = mensaje;

        if (!exito) return View(modelo);

        return RedirectToAction("Detalles", new { id = clienteId });
    }

    // GET: /Clientes/Editar/5
    public async Task<IActionResult> Editar(int? id)
    {
        if (id == null) return NotFound();

        var detalle = await _clienteService.ObtenerDetalleAsync(id.Value);
        if (detalle == null) return NotFound();

        return View(new ClienteEditarViewModel
        {
            ClienteId = detalle.ClienteId,
            Nombre = detalle.Nombre,
            FechaNacimiento = detalle.FechaNacimiento,
            NotasMedicas = detalle.NotasMedicas,
            NombreTutor1 = detalle.NombreTutor1,
            TelefonoTutor1 = detalle.TelefonoTutor1,
            NombreTutor2 = detalle.NombreTutor2,
            TelefonoTutor2 = detalle.TelefonoTutor2
        });
    }

    // POST: /Clientes/Editar/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, ClienteEditarViewModel modelo)
    {
        if (id != modelo.ClienteId) return NotFound();

        var (exito, mensaje) = await _clienteService.ActualizarAsync(modelo);
        TempData[exito ? "Mensaje" : "Error"] = mensaje;

        if (!exito) return View(modelo);

        return RedirectToAction("Detalles", new { id });
    }

    // POST: /Clientes/CapturarMuestra
    /// <summary>
    /// Simula la captura de una muestra de huella desde el navegador.
    /// Cuando exista el servicio local del SDK DigitalPersona (RNF3), este endpoint
    /// delegar?? en ??l; el resto de la aplicaci??n no cambia.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CapturarMuestra()
    {
        var muestra = await _biometria.CapturarMuestraAsync();
        return Json(new { ok = true, muestraBase64 = Convert.ToBase64String(muestra) });
    }
}

