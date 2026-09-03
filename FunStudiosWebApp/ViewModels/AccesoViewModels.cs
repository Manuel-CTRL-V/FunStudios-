namespace FunStudiosWebApp.ViewModels;

/// <summary>
/// Resultado de un escaneo en el checkpoint (RF5). Contiene todo lo que la
/// recepción necesita para decidir el siguiente paso sin recargar la página.
/// </summary>
public class ResultadoEscaneoViewModel
{
    public bool Encontrado { get; set; }

    /// <summary>Mensaje para mostrar bajo el escáner (éxito o motivo del fallo).</summary>
    public string Mensaje { get; set; } = string.Empty;

    // Datos del cliente identificado

    public int? ClienteId { get; set; }
    public string? Nombre { get; set; }
    public string? ApellidosFamilia { get; set; }
    public int Edad { get; set; }
    public string Inicial { get; set; } = "?";
    public string? NotasMedicas { get; set; }

    // Control de presencia

    /// <summary>El cliente tiene una entrada abierta (está dentro del parque).</summary>
    public bool EstaDentro { get; set; }

    public int? EntradaAbiertaId { get; set; }

    public DateTime? HoraEntradaActual { get; set; }

    // Fidelidad

    public byte ConteoChecks { get; set; }
    public int ChecksRestantes { get; set; }
    public int EntradasGratisDisponibles { get; set; }

    /// <summary>RF9: ya se otorgó check por una entrada Day Pass hoy.</summary>
    public bool YaCobroCheckHoy { get; set; }

    // Membresía vigente

    public bool MembresiaVigente { get; set; }
    public string? NombreMembresiaVigente { get; set; }
    public DateOnly? VenceMembresia { get; set; }

    // Tutores registrados de la familia (para elegir quién acompaña en la entrada)

    public string? NombreTutor1 { get; set; }
    public string? TelefonoTutor1 { get; set; }
    public string? NombreTutor2 { get; set; }
    public string? TelefonoTutor2 { get; set; }

    /// <summary>Opciones de acceso disponibles para registrar una entrada.</summary>
    public List<OpcionAccesoViewModel> OpcionesEntrada { get; set; } = new();
}

/// <summary>Una opción de acceso mostrada al recepcionista (Day Pass, Gratis, Membresía).</summary>
public class OpcionAccesoViewModel
{
    public int TipoAccesoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    /// <summary>Si requiere pago, el formulario pedirá método de pago y monto.</summary>
    public bool RequierePago { get; set; }
}

/// <summary>Datos que envía el formulario "Registrar entrada" del checkpoint.</summary>
public class RegistrarEntradaRequest
{
    public int ClienteId { get; set; }
    public int TipoAccesoId { get; set; }
    public int? MetodoPagoId { get; set; }
    public decimal Monto { get; set; }

    /// <summary>"tutor1" (por defecto) | "tutor2" | "otro".</summary>
    public string? SeleccionTutor { get; set; }

    /// <summary>Solo cuando SeleccionTutor = "otro": datos del acompañante no registrado.</summary>
    public string? TutorOtroNombre { get; set; }
    public string? TutorOtroTelefono { get; set; }
}

/// <summary>Respuesta al registrar una entrada (incluye alerta de premio RF10).</summary>
public class ResultadoRegistroEntradaViewModel
{
    public bool Exito { get; set; }
    public string Mensaje { get; set; } = string.Empty;

    /// <summary>RF10: true cuando el 8vo check acredita una entrada gratis.</summary>
    public bool PremioGanado { get; set; }

    public string? NombreCliente { get; set; }
    public byte ConteoChecks { get; set; }
}
