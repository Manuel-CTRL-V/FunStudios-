namespace FunStudiosWebApp.Models;

/// <summary>
/// Representa a un empleado con acceso al sistema.
/// La autenticación se realiza mediante cookies con claims de rol (RF11).
/// </summary>
public class Usuario
{
    public int UsuarioId { get; set; }

    public int RolId { get; set; }

    public string Nombres { get; set; } = string.Empty;

    public string Apellidos { get; set; } = string.Empty;

    /// <summary>Nombre de usuario para el login.</summary>
    public string NombreUsuario { get; set; } = string.Empty;

    /// <summary>Hash PBKDF2 de la contraseña. Nunca se almacena texto plano.</summary>
    public string PasswordHash { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;

    // Propiedades calculadas

    /// <summary>Nombre completo para mostrar en la interfaz.</summary>
    public string NombreCompleto => $"{Nombres} {Apellidos}";

    // Relaciones

    public Rol? Rol { get; set; }
}
