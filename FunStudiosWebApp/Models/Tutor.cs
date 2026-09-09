namespace FunStudiosWebApp.Models;

/// <summary>
/// Tutor responsable de un grupo familiar (RF2). Una familia puede tener varios
/// tutores; exactamente uno es el principal (EsPrincipal).
/// Si el tutor tiene enrolada su huella (HuellaTemplate), habilita el ingreso
/// de los niños menores de 2 años de la familia (RF4).
/// </summary>
public class Tutor
{
    public int TutorId { get; set; }

    public int FamiliaId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Telefono { get; set; } = string.Empty;

    /// <summary>true para el tutor principal de la familia.</summary>
    public bool EsPrincipal { get; set; }

    /// <summary>
    /// Template biométrico del tutor (RF4), opcional, encriptado en producción (RNF4).
    /// Permite el ingreso de los niños menores de 2 años de esta familia.
    /// </summary>
    public byte[]? HuellaTemplate { get; set; }

    public Familia? Familia { get; set; }
}