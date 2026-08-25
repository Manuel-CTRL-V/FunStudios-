namespace FunStudiosWebApp.ViewModels;

/// <summary>Fila de la tabla "Gestión de Clientes".</summary>
public class ClienteListadoItemViewModel
{
    public int ClienteId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Inicial { get; set; } = "?";
    public int Edad { get; set; }
    public string ApellidosFamilia { get; set; } = string.Empty;
    public bool TieneNotaMedica { get; set; }

    // Fidelidad (RF8)
    public byte ConteoChecks { get; set; }
    public int EntradasGratisDisponibles { get; set; }

    /// <summary>Estado mostrado en la tabla: Activo (dentro del parque) / Fuera.</summary>
    public bool EstaDentro { get; set; }

    /// <summary>Texto legible de la última visita: "Hoy, 09:15 a.m." o fecha corta.</summary>
    public string UltimaEntradaTexto { get; set; } = "Sin visitas";
}

/// <summary>Perfil completo del cliente para la pantalla de detalles.</summary>
public class ClienteDetalleViewModel
{
    public int ClienteId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Inicial { get; set; } = "?";
    public DateOnly FechaNacimiento { get; set; }
    public int Edad { get; set; }
    public string? NotasMedicas { get; set; }
    public bool HuellaEnrolada { get; set; }
    public bool HuellaTutorEnrolada { get; set; }

    // Familia
    public int FamiliaId { get; set; }
    public string ApellidosFamilia { get; set; } = string.Empty;
    public string TelefonoTutor1 { get; set; } = string.Empty;
    public string? TelefonoTutor2 { get; set; }

    // Fidelidad
    public byte ConteoChecks { get; set; }
    public int EntradasGratisDisponibles { get; set; }
    public DateTime? FechaUltimaActualizacionFidelidad { get; set; }

    // Presencia
    public bool EstaDentro { get; set; }

    public List<EntradaItemViewModel> EntradasRecientes { get; set; } = new();
    public List<MembresiaItemViewModel> Membresias { get; set; } = new();
    public List<VentaItemViewModel> VentasRecientes { get; set; } = new();
}

/// <summary>Resumen de una entrada para listados y bitácora (RF12).</summary>
public class EntradaItemViewModel
{
    public int EntradaId { get; set; }
    public int ClienteId { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public string ApellidosFamilia { get; set; } = string.Empty;
    public DateTime FechaHoraEntrada { get; set; }
    public DateTime? FechaHoraSalida { get; set; }
    public string TipoAccesoNombre { get; set; } = string.Empty;
    public int TipoAccesoId { get; set; }
    public string? TutorOtro { get; set; }
    public decimal? MontoVenta { get; set; }

    public bool EstaDentro => FechaHoraSalida == null;

    /// <summary>Duración formateada de la visita, si ya salió.</summary>
    public string DuracionTexto
    {
        get
        {
            if (FechaHoraSalida == null) return "En el parque";
            var duracion = FechaHoraSalida.Value - FechaHoraEntrada;
            return $"{(int)duracion.TotalHours} h {duracion.Minutes} min";
        }
    }
}

/// <summary>Resumen de una membresía para el detalle del cliente.</summary>
public class MembresiaItemViewModel
{
    public int MembresiaId { get; set; }
    public string NombreMembresia { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }
    public decimal Precio { get; set; }
    public string Estado { get; set; } = string.Empty;
    public bool Vigente { get; set; }
}

/// <summary>Resumen de una venta para listados e historial de facturas.</summary>
public class VentaItemViewModel
{
    public int VentaId { get; set; }
    public DateTime FechaVenta { get; set; }
    public int ClienteId { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public string MetodoPagoNombre { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public string Descripcion { get; set; } = string.Empty;
}

/// <summary>
/// Formulario de registro + enrolamiento (RF1, RF2, RF3, RF4).
/// Los templates llegan como Base64 desde la captura simulada del navegador.
/// </summary>
public class ClienteCrearViewModel
{
    // Familia: nueva o existente

    public bool UsarFamiliaExistente { get; set; }

    public int? FamiliaExistenteId { get; set; }

    public List<FamiliaOpcionViewModel> FamiliasExistentes { get; set; } = new();

    // Datos de la familia nueva

    public string ApellidosFamilia { get; set; } = string.Empty;
    public string TelefonoTutor1 { get; set; } = string.Empty;
    public string? TelefonoTutor2 { get; set; }

    // Datos del niño

    public string Nombre { get; set; } = string.Empty;
    public DateOnly? FechaNacimiento { get; set; }
    public string? NotasMedicas { get; set; }

    // Enrolamiento biométrico

    public string? HuellaNinoTemplateBase64 { get; set; }
    public bool ConHuellaTutor { get; set; }
    public string? HuellaTutorTemplateBase64 { get; set; }
}

/// <summary>Formulario de edición de datos básicos del cliente y su familia.</summary>
public class ClienteEditarViewModel
{
    public int ClienteId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public DateOnly? FechaNacimiento { get; set; }
    public string? NotasMedicas { get; set; }
    public string TelefonoTutor1 { get; set; } = string.Empty;
    public string? TelefonoTutor2 { get; set; }
}

/// <summary>Opción para el dropdown "familia existente" del formulario.</summary>
public class FamiliaOpcionViewModel
{
    public int FamiliaId { get; set; }
    public string Etiqueta { get; set; } = string.Empty;
}
