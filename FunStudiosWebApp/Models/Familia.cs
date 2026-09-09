namespace FunStudiosWebApp.Models;

/// <summary>
/// Grupo familiar. Contiene los datos de contacto de los tutores responsables (RF2),
/// que se guardan en la tabla Tutores (una familia puede tener N tutores).
/// Una familia puede tener varios clientes (niños).
/// </summary>
public class Familia
{
    public int FamiliaId { get; set; }

    public string ApellidosFamilia { get; set; } = string.Empty;

    /// <summary>
    /// Código único de la familia en formato secuencial "FAM-001".
    /// Se genera automáticamente al crear la familia; índice único en BD.
    /// </summary>
    public string? CodigoFamilia { get; set; }

    // Relaciones

    public ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();

    /// <summary>Tutores de la familia (uno es el principal, marcado con EsPrincipal).</summary>
    public ICollection<Tutor> Tutores { get; set; } = new List<Tutor>();
}