using FunStudiosWebApp.Services;
using FunStudiosWebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FunStudiosWebApp.Controllers;

/// <summary>
/// Configuración general del sistema: tipos de acceso (precio/descripción/activo),
/// métodos de pago y parámetros clave-valor. Solo accesible para Administradores.
/// </summary>
[Authorize(Roles = "Administrador")]
public class ConfiguracionController : Controller
{
    private readonly IConfiguracionService _configuracionService;

    public ConfiguracionController(IConfiguracionService configuracionService)
    {
        _configuracionService = configuracionService;
    }

    // GET: /Configuracion
    public async Task<IActionResult> Index()
    {
        var modelo = await _configuracionService.ObtenerResumenAsync();
        return View(modelo);
    }

    // ---------- Tipos de acceso ----------

    // GET: /Configuracion/EditarTipoAcceso/5
    public async Task<IActionResult> EditarTipoAcceso(int? id)
    {
        if (id == null) return NotFound();

        var modelo = await _configuracionService.ObtenerTipoAccesoParaEditarAsync(id.Value);
        if (modelo == null) return NotFound();

        return View(modelo);
    }

    // POST: /Configuracion/EditarTipoAcceso/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarTipoAcceso(int id, TipoAccesoEditarViewModel modelo)
    {
        if (id != modelo.TipoAccesoId) return NotFound();

        var (exito, mensaje) = await _configuracionService.GuardarTipoAccesoAsync(modelo);
        TempData[exito ? "Mensaje" : "Error"] = mensaje;

        if (!exito) return View(modelo);

        return RedirectToAction(nameof(Index));
    }

    // ---------- Métodos de pago ----------

    // GET: /Configuracion/CrearMetodoPago
    public IActionResult CrearMetodoPago()
    {
        return View(new MetodoPagoFormViewModel());
    }

    // POST: /Configuracion/CrearMetodoPago
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearMetodoPago(MetodoPagoFormViewModel modelo)
    {
        var (exito, mensaje) = await _configuracionService.CrearMetodoPagoAsync(modelo.NombreMetodo);
        TempData[exito ? "Mensaje" : "Error"] = mensaje;

        if (!exito) return View(modelo);

        return RedirectToAction(nameof(Index));
    }

    // GET: /Configuracion/EditarMetodoPago/5
    public async Task<IActionResult> EditarMetodoPago(int? id)
    {
        if (id == null) return NotFound();

        var modelo = await _configuracionService.ObtenerMetodoPagoParaEditarAsync(id.Value);
        if (modelo == null) return NotFound();

        return View(modelo);
    }

    // POST: /Configuracion/EditarMetodoPago/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarMetodoPago(int id, MetodoPagoFormViewModel modelo)
    {
        if (id != modelo.MetodoPagoId) return NotFound();

        var (exito, mensaje) = await _configuracionService.ActualizarMetodoPagoAsync(id, modelo.NombreMetodo);
        TempData[exito ? "Mensaje" : "Error"] = mensaje;

        if (!exito) return View(modelo);

        return RedirectToAction(nameof(Index));
    }

    // POST: /Configuracion/EliminarMetodoPago/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarMetodoPago(int id)
    {
        var (exito, mensaje) = await _configuracionService.EliminarMetodoPagoAsync(id);
        TempData[exito ? "Mensaje" : "Error"] = mensaje;

        return RedirectToAction(nameof(Index));
    }

    // ---------- Parámetros ----------

    // GET: /Configuracion/EditarParametro/5
    public async Task<IActionResult> EditarParametro(int? id)
    {
        if (id == null) return NotFound();

        var modelo = await _configuracionService.ObtenerParametroParaEditarAsync(id.Value);
        if (modelo == null) return NotFound();

        return View(modelo);
    }

    // POST: /Configuracion/EditarParametro/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarParametro(int id, ParametroEditarViewModel modelo)
    {
        if (id != modelo.ParametroId) return NotFound();

        var (exito, mensaje) = await _configuracionService.GuardarParametroAsync(modelo);
        TempData[exito ? "Mensaje" : "Error"] = mensaje;

        if (!exito) return View(modelo);

        return RedirectToAction(nameof(Index));
    }
}