using FunStudiosWebApp.Models;
using FunStudiosWebApp.ViewModels;

namespace FunStudiosWebApp.Services;

/// <summary>Gestión de usuarios del sistema y autenticación (RF11).</summary>
public interface IUsuarioService
{
    /// <summary>Valida credenciales para el login. Devuelve null si son inválidas o el usuario está inactivo.</summary>
    Task<Usuario?> ValidarCredencialesAsync(string nombreUsuario, string password);

    Task<List<UsuarioListItemViewModel>> ObtenerTodosAsync();

    Task<ResumenUsuariosViewModel> ObtenerResumenAsync();

    Task<List<RolOpcionViewModel>> ObtenerRolesAsync();

    Task<UsuarioFormViewModel?> ObtenerParaEditarAsync(int usuarioId);

    /// <summary>Crea un usuario nuevo con contraseña hasheada.</summary>
    Task<(bool Exito, string Mensaje)> CrearAsync(UsuarioFormViewModel modelo);

    /// <summary>Actualiza datos, rol, estado y opcionalmente restablece la contraseña.</summary>
    Task<(bool Exito, string Mensaje)> ActualizarAsync(UsuarioFormViewModel modelo);
}
