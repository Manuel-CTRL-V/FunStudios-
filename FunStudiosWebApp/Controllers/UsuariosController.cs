using FunStudiosWebApp.Services;
using FunStudiosWebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FunStudiosWebApp.Controllers;

/// <summary>Gestión de usuarios del sistema. Solo accesible para Administradores (RF11).</summary>
[Authorize(Roles = "Administrador")]
public class UsuariosController : Controller
{
    private readonly IUsuarioService _usuarioService;

    public UsuariosController(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    // GET: /Usuarios
    public async Task<IActionResult> Index()
    {
        var modelo = await _usuarioService.ObtenerTodosAsync();
        ViewData["Resumen"] = await _usuarioService.ObtenerResumenAsync();
        return View(modelo);
    }

    // GET: /Usuarios/Crear
    public async Task<IActionResult> Crear()
    {
        var modelo = new UsuarioFormViewModel
        {
            Roles = await _usuarioService.ObtenerRolesAsync()
        };
        return View(modelo);
    }

    // POST: /Usuarios/Crear
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(UsuarioFormViewModel modelo)
    {
        modelo.Roles = await _usuarioService.ObtenerRolesAsync();

        var (exito, mensaje) = await _usuarioService.CrearAsync(modelo);
        TempData[exito ? "Mensaje" : "Error"] = mensaje;

        if (!exito) return View(modelo);

        return RedirectToAction(nameof(Index));
    }

    // GET: /Usuarios/Editar/5
    public async Task<IActionResult> Editar(int? id)
    {
        if (id == null) return NotFound();

        var modelo = await _usuarioService.ObtenerParaEditarAsync(id.Value);
        if (modelo == null) return NotFound();

        return View(modelo);
    }

    // POST: /Usuarios/Editar/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, UsuarioFormViewModel modelo)
    {
        if (id != modelo.UsuarioId) return NotFound();

        modelo.Roles = await _usuarioService.ObtenerRolesAsync();

        var (exito, mensaje) = await _usuarioService.ActualizarAsync(modelo);
        TempData[exito ? "Mensaje" : "Error"] = mensaje;

        if (!exito) return View(modelo);

        return RedirectToAction(nameof(Index));
    }
}
