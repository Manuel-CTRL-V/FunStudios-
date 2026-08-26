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

    /// <summary>
    /// Tutor responsable que acompañó al niño en esta entrada (RF6).
    /// Se guarda una "foto" del nombre y teléfono al momento de registrar la entrada:
    /// puede ser el tutor 1, el tutor 2 o un acompañante no registrado.
    /// </summary>
    public string? TutorNombre { get; set; }

    public string? TutorTelefono { get; set; }

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
