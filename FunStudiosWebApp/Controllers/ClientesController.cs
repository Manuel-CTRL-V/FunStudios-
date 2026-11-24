using FunStudiosWebApp.Services;
using FunStudiosWebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FunStudiosWebApp.Controllers;

/// <summary>Gestión de clientes: listado, perfil, registro y edición (RF1, RF2).</summary>
[Authorize]
public class ClientesController : Controller
{
    private readonly IClienteService _clienteService;

    public ClientesController(IClienteService clienteService)
    {
        _clienteService = clienteService;
    }

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
    public async Task<IActionResult> Crear(int? familiaId)
    {
        var familias = await _clienteService.ObtenerFamiliasAsync();
        var modelo = new ClienteCrearViewModel
        {
            FamiliasExistentes = familias,
            FamiliaExistenteId = familiaId
        };
        return View(modelo);
    }

    // POST: /Clientes/Crear
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(ClienteCrearViewModel modelo)
    {
        modelo.FamiliasExistentes = await _clienteService.ObtenerFamiliasAsync();

        if (!ModelState.IsValid) return View(modelo);

        var (exito, mensaje, clienteId) = await _clienteService.RegistrarAsync(modelo);

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
            FechaNacimiento = detalle.TieneFechaNacimiento ? detalle.FechaNacimiento : null,
            NotasMedicas = detalle.NotasMedicas,
            Nacionalidad = detalle.Nacionalidad,
            FamiliaId = detalle.FamiliaId
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
}
