namespace FunStudiosWebApp.ViewModels;

/// <summary>Fila del directorio de personal (pantalla Usuarios).</summary>
public class UsuarioListItemViewModel
{
    public int UsuarioId { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Iniciales { get; set; } = "?";
    public string NombreUsuario { get; set; } = string.Empty;
    public int RolId { get; set; }
    public string RolNombre { get; set; } = string.Empty;
    public bool Activo { get; set; }
}

/// <summary>Formulario de creación/edición de usuarios (RF11, solo Administrador).</summary>
public class UsuarioFormViewModel
{
    public int UsuarioId { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string NombreUsuario { get; set; } = string.Empty;

    /// <summary>Solo se escribe al crear o al restablecer contraseña; nunca se muestra.</summary>
    public string? Password { get; set; }

    public int RolId { get; set; }
    public bool Activo { get; set; } = true;

    public bool EsEdicion => UsuarioId > 0;

    public List<RolOpcionViewModel> Roles { get; set; } = new();
}

/// <summary>Opción para el dropdown de roles.</summary>
public class RolOpcionViewModel
{
    public int RolId { get; set; }
    public string NombreRol { get; set; } = string.Empty;
}

/// <summary>Tarjetas resumen de la pantalla de usuarios.</summary>
public class ResumenUsuariosViewModel
{
    public int TotalUsuarios { get; set; }
    public int Activos { get; set; }
    public int TotalAdministradores { get; set; }
    public int TotalRecepcion { get; set; }
}
