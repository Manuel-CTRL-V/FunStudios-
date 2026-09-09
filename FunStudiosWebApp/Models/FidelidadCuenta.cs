namespace FunStudiosWebApp.Models;

/// <summary>
/// Controla los checks de fidelidad acumulados por cada cliente (RF8, RF9, RF10).
/// Clientes 1 → 1 Fidelidad_Cuentas. Al completar los checks configurados se acredita
/// una entrada gratis y el ciclo reinicia a cero. La meta está en Parametros
/// ("Fidelidad.ChecksParaPremio"), por eso ya no hay constante dura aquí.
/// </summary>
public class FidelidadCuenta
{
    public int FidelidadId { get; set; }

    public int ClienteId { get; set; }

    /// <summary>Conteo de checks acumulados. Solo acumula con accesos Day Pass (RF8).</summary>
    public byte ConteoChecks { get; set; }

    /// <summary>Saldo de entradas gratis disponibles para canjear.</summary>
    public int EntradasGratisGanadas { get; set; }

    public DateTime? FechaUltimaActualizacion { get; set; }

    // Relaciones

    public Cliente? Cliente { get; set; }
}
