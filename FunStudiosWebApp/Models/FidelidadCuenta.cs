namespace FunStudiosWebApp.Models;

/// <summary>
/// Controla los checks de fidelidad acumulados por cada cliente (RF8, RF9, RF10).
/// Clientes 1 → 1 Fidelidad_Cuentas. Al llegar a 8 checks se acredita una entrada gratis y el ciclo reinicia a cero.
/// </summary>
public class FidelidadCuenta
{
    /// <summary>Checks necesarios para ganar una entrada gratis.</summary>
    public const byte ChecksParaPremio = 8;

    public int FidelidadId { get; set; }

    public int ClienteId { get; set; }

    /// <summary>Conteo de checks de 0 a 8. Solo acumula con accesos Day Pass (RF8).</summary>
    public byte ConteoChecks { get; set; }

    /// <summary>Saldo de entradas gratis disponibles para canjear.</summary>
    public int EntradasGratisGanadas { get; set; }

    public DateTime? FechaUltimaActualizacion { get; set; }

    // Propiedades calculadas

    /// <summary>Faltante para la próxima entrada gratis.</summary>
    public int ChecksRestantes => Math.Max(0, ChecksParaPremio - ConteoChecks);

    // Relaciones

    public Cliente? Cliente { get; set; }
}
