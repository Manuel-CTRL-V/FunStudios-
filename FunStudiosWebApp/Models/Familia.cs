namespace FunStudiosWebApp.Models;

/// <summary>
/// Grupo familiar. Contiene los datos de contacto de hasta dos tutores responsables (RF2).
/// Una familia puede tener varios clientes (niños).
/// </summary>
public class Familia
{
    public int FamiliaId { get; set; }

    public string ApellidosFamilia { get; set; } = string.Empty;

    public string TelefonoTutor1 { get; set; } = string.Empty;

    public string? TelefonoTutor2 { get; set; }

    /// <summary>
    /// Template biométrico del tutor, opcional (RF4), encriptado en producción (RNF4).
    /// </summary>
    public byte[]? HuellaTutorTemplate { get; set; }

    // Relaciones

    public ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();
}
