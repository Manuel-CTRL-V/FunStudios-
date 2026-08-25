namespace FunStudiosWebApp.Models;

/// <summary>
/// Registra cada entrada de un cliente al parque, con su hora de entrada y de salida (RF5, RF6, RF7).
/// FechaHoraSalida queda null mientras el cliente esté dentro del parque.
/// </summary>
public class EntradaHistorial
{
    public int EntradaId { get; set; }

    /// <summary>Familia responsable del cliente en el momento de la entrada.</summary>
    public int FamiliaId { get; set; }

    public int ClienteId { get; set; }

    public DateTime FechaHoraEntrada { get; set; }

    public DateTime? FechaHoraSalida { get; set; }

    /// <summary>DayPass | FidelidadGratis | Membresía</summary>
    public int TipoAccesoId { get; set; }

    /// <summary>Null cuando la entrada no utiliza una membresía.</summary>
    public int? MembresiaId { get; set; }

    /// <summary>Null cuando la entrada no proviene de una venta directa (ej. Day Pass siempre genera venta).</summary>
    public int? VentaId { get; set; }

    /// <summary>Acompañante que no corresponde a los tutores registrados (abuela, tío, etc.).</summary>
    public string? TutorOtro { get; set; }

    // Propiedades calculadas

    /// <summary>El cliente sigue dentro del parque mientras no se registre su salida (RF7).</summary>
    public bool EstaDentro => FechaHoraSalida == null;

    // Relaciones

    public Cliente? Cliente { get; set; }

    public Familia? Familia { get; set; }

    public TipoAcceso? TipoAcceso { get; set; }

    public Membresia? Membresia { get; set; }

    public Venta? Venta { get; set; }
}
