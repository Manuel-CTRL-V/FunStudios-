using FunStudiosWebApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FunStudiosWebApp.Controllers;

/// <summary>
/// Apartado de familias: listado general, perfil familiar con sus tutores
/// y los niños asociados (RF2).
/// </summary>
[Authorize]
public class FamiliasController : Controller
{
    private readonly IFamiliaService _familiaService;

    public FamiliasController(IFamiliaService familiaService)
    {
        _familiaService = familiaService;
    }

    // GET: /Familias
    public async Task<IActionResult> Index(string? busqueda, string? estado)
    {
        ViewData["Busqueda"] = busqueda;
        ViewData["Estado"] = estado;
        var familias = await _familiaService.ObtenerListadoAsync(busqueda, estado);
        return View(familias);
    }

    // GET: /Familias/Detalles/5
    public async Task<IActionResult> Detalles(int? id)
    {
        if (id == null) return NotFound();

        var familia = await _familiaService.ObtenerDetalleAsync(id.Value);
        if (familia == null) return NotFound();

        return View(familia);
    }

    // POST: /Familias/AgregarTutor
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AgregarTutor(int familiaId, string nombre, string telefono)
    {
        var (exito, mensaje) = await _familiaService.AgregarTutorAsync(familiaId, nombre, telefono);
        TempData[exito ? "Mensaje" : "Error"] = mensaje;

        return RedirectToAction("Detalles", new { id = familiaId });
    }

    // POST: /Familias/ActualizarTutor
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ActualizarTutor(int tutorId, int familiaId, string nombre, string telefono, bool esPrincipal)
    {
        var (exito, mensaje) = await _familiaService.ActualizarTutorAsync(tutorId, nombre, telefono, esPrincipal);
        TempData[exito ? "Mensaje" : "Error"] = mensaje;

        return RedirectToAction("Detalles", new { id = familiaId });
    }

    // POST: /Familias/EliminarTutor
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarTutor(int tutorId, int familiaId)
    {
        var (exito, mensaje) = await _familiaService.EliminarTutorAsync(tutorId);
        TempData[exito ? "Mensaje" : "Error"] = mensaje;

        return RedirectToAction("Detalles", new { id = familiaId });
    }
}