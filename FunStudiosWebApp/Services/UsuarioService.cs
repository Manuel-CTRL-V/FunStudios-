using FunStudiosWebApp.Data;
using FunStudiosWebApp.Models;
using FunStudiosWebApp.Security;
using FunStudiosWebApp.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace FunStudiosWebApp.Services;

/// <summary>Implementación de la gestión de usuarios (RF11).</summary>
public class UsuarioService : IUsuarioService
{
    private readonly FunStudiosContext _context;

    public UsuarioService(FunStudiosContext context)
    {
        _context = context;
    }

    public async Task<Usuario?> ValidarCredencialesAsync(string nombreUsuario, string password)
    {
        if (string.IsNullOrWhiteSpace(nombreUsuario) || string.IsNullOrWhiteSpace(password))
            return null;

        var usuario = await _context.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario.Trim().ToLower());

        if (usuario == null || !usuario.Activo)
            return null;

        return PasswordHasher.Verificar(password, usuario.PasswordHash) ? usuario : null;
    }

    public async Task<List<UsuarioListItemViewModel>> ObtenerTodosAsync()
    {
        return await _context.Usuarios
            .Include(u => u.Rol)
            .OrderByDescending(u => u.Activo)
            .ThenBy(u => u.Apellidos)
            .Select(u => new UsuarioListItemViewModel
            {
                UsuarioId = u.UsuarioId,
                NombreCompleto = u.NombreCompleto,
                Iniciales = ObtenerIniciales(u.Nombres, u.Apellidos),
                NombreUsuario = u.NombreUsuario,
                RolId = u.RolId,
                RolNombre = u.Rol!.NombreRol,
                Activo = u.Activo
            })
            .ToListAsync();
    }

    private static string ObtenerIniciales(string nombres, string apellidos)
    {
        var inicialNombre = string.IsNullOrEmpty(nombres) ? "?" : nombres[..1].ToUpper();
        var inicialApellido = string.IsNullOrEmpty(apellidos) ? "" : apellidos[..1].ToUpper();
        return $"{inicialNombre}{inicialApellido}";
    }

    public async Task<ResumenUsuariosViewModel> ObtenerResumenAsync()
    {
        return await _context.Usuarios
            .GroupBy(u => true)
            .Select(g => new ResumenUsuariosViewModel
            {
                TotalUsuarios = g.Count(),
                Activos = g.Count(u => u.Activo),
                TotalAdministradores = g.Count(u => u.RolId == Rol.Administrador),
                TotalRecepcion = g.Count(u => u.RolId == Rol.Recepcion)
            })
            .FirstOrDefaultAsync() ?? new ResumenUsuariosViewModel();
    }

    public async Task<List<RolOpcionViewModel>> ObtenerRolesAsync()
    {
        return await _context.Roles
            .OrderBy(r => r.RolId)
            .Select(r => new RolOpcionViewModel { RolId = r.RolId, NombreRol = r.NombreRol })
            .ToListAsync();
    }

    public async Task<UsuarioFormViewModel?> ObtenerParaEditarAsync(int usuarioId)
    {
        var usuario = await _context.Usuarios.FindAsync(usuarioId);
        if (usuario == null) return null;

        return new UsuarioFormViewModel
        {
            UsuarioId = usuario.UsuarioId,
            Nombres = usuario.Nombres,
            Apellidos = usuario.Apellidos,
            NombreUsuario = usuario.NombreUsuario,
            RolId = usuario.RolId,
            Activo = usuario.Activo,
            Roles = await ObtenerRolesAsync()
        };
    }

    public async Task<(bool Exito, string Mensaje)> CrearAsync(UsuarioFormViewModel modelo)
    {
        var validacion = ValidarFormulario(modelo, esCreacion: true);
        if (validacion != null) return (false, validacion);

        var nombreNormalizado = modelo.NombreUsuario.Trim().ToLower();

        var duplicado = await _context.Usuarios.AnyAsync(u => u.NombreUsuario == nombreNormalizado);
        if (duplicado)
            return (false, $"El nombre de usuario '{nombreNormalizado}' ya está en uso.");

        _context.Usuarios.Add(new Usuario
        {
            Nombres = modelo.Nombres.Trim(),
            Apellidos = modelo.Apellidos.Trim(),
            NombreUsuario = nombreNormalizado,
            PasswordHash = PasswordHasher.Hash(modelo.Password!),
            RolId = modelo.RolId,
            Activo = modelo.Activo
        });

        await _context.SaveChangesAsync();
        return (true, $"Usuario '{modelo.Nombres} {modelo.Apellidos}' creado correctamente.");
    }

    public async Task<(bool Exito, string Mensaje)> ActualizarAsync(UsuarioFormViewModel modelo)
    {
        var validacion = ValidarFormulario(modelo, esCreacion: false);
        if (validacion != null) return (false, validacion);

        var usuario = await _context.Usuarios.FindAsync(modelo.UsuarioId);
        if (usuario == null) return (false, "El usuario no existe.");

        var nombreNormalizado = modelo.NombreUsuario.Trim().ToLower();

        var duplicado = await _context.Usuarios
            .AnyAsync(u => u.NombreUsuario == nombreNormalizado && u.UsuarioId != modelo.UsuarioId);
        if (duplicado)
            return (false, $"El nombre de usuario '{nombreNormalizado}' ya está en uso por otra persona.");

        // Protección: no dejar al sistema sin administradores activos
        if (!modelo.Activo && usuario.RolId == Rol.Administrador)
        {
            var otrosAdminsActivos = await _context.Usuarios
                .CountAsync(u => u.UsuarioId != modelo.UsuarioId && u.RolId == Rol.Administrador && u.Activo);
            if (otrosAdminsActivos == 0)
                return (false, "No se puede desactivar al único administrador activo del sistema.");
        }

        usuario.Nombres = modelo.Nombres.Trim();
        usuario.Apellidos = modelo.Apellidos.Trim();
        usuario.NombreUsuario = nombreNormalizado;
        usuario.RolId = modelo.RolId;
        usuario.Activo = modelo.Activo;

        // Restablecimiento opcional de contraseña
        if (!string.IsNullOrWhiteSpace(modelo.Password))
            usuario.PasswordHash = PasswordHasher.Hash(modelo.Password);

        await _context.SaveChangesAsync();
        return (true, "Usuario actualizado correctamente.");
    }

    private static string? ValidarFormulario(UsuarioFormViewModel modelo, bool esCreacion)
    {
        if (string.IsNullOrWhiteSpace(modelo.Nombres)) return "Los nombres son obligatorios.";
        if (string.IsNullOrWhiteSpace(modelo.Apellidos)) return "Los apellidos son obligatorios.";
        if (string.IsNullOrWhiteSpace(modelo.NombreUsuario)) return "El nombre de usuario es obligatorio.";

        if (esCreacion && string.IsNullOrWhiteSpace(modelo.Password))
            return "La contraseña es obligatoria para usuarios nuevos.";

        if (!string.IsNullOrWhiteSpace(modelo.Password) && modelo.Password.Length < 6)
            return "La contraseña debe tener al menos 6 caracteres.";

        if (!_rolesValidos.Contains(modelo.RolId)) return "Seleccione un rol válido.";

        return null;
    }

    private static readonly int[] _rolesValidos = { Rol.Administrador, Rol.Recepcion };
}
