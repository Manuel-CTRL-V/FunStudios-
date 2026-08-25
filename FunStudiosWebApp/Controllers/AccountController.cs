using System.Security.Claims;
using FunStudiosWebApp.Models;
using FunStudiosWebApp.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FunStudiosWebApp.Controllers;

/// <summary>Login y cierre de sesión del personal (RF11).</summary>
public class AccountController : Controller
{
    private readonly IUsuarioService _usuarioService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(IUsuarioService usuarioService, ILogger<AccountController> logger)
    {
        _usuarioService = usuarioService;
        _logger = logger;
    }

    // GET: /Account/Login
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        // Si ya hay sesión activa, ir directo al panel
        if (User.Identity!.IsAuthenticated)
            return RedirectToAction("Index", "Dashboard");

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    // POST: /Account/Login
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string nombreUsuario, string password, string? returnUrl = null)
    {
        var usuario = await _usuarioService.ValidarCredencialesAsync(nombreUsuario ?? string.Empty, password ?? string.Empty);

        if (usuario == null)
        {
            ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos, o cuenta inactiva.");
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // Claims de identidad: nombre, id y rol para la autorización por roles
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.UsuarioId.ToString()),
            new(ClaimTypes.Name, usuario.NombreCompleto),
            new(ClaimTypes.Role, usuario.Rol?.NombreRol ?? string.Empty)
        };

        var identidad = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identidad);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
            new AuthenticationProperties { IsPersistent = false });

        _logger.LogInformation("El usuario {Usuario} inició sesión.", usuario.NombreUsuario);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Dashboard");
    }

    // POST: /Account/Logout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }

    // GET: /Account/AccesoDenegado
    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccesoDenegado()
    {
        return View();
    }
}
