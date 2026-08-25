namespace FunStudiosWebApp.Models;

/// <summary>
/// Define cómo se obtiene el acceso al parque.
/// Valores contemplados (IDs fijos usados por la lógica de fidelidad):
/// 1 - DayPass | 2 - FidelidadGratis | 3 - Membresía
/// </summary>
public class TipoAcceso
{
    public const int DayPass = 1;
    public const int FidelidadGratis = 2;
    public const int Membresia = 3;

    public int TipoAccesoId { get; set; }

    public string NombreAcceso { get; set; } = string.Empty;
}
