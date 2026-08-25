namespace FunStudiosWebApp.Models;

/// <summary>
/// Define los roles de los usuarios del sistema (RF11).
/// Valores contemplados (IDs fijos): 1 - Administrador | 2 - Recepción
/// </summary>
public class Rol
{
    public const int Administrador = 1;
    public const int Recepcion = 2;

    public int RolId { get; set; }

    public string NombreRol { get; set; } = string.Empty;
}
