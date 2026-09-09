namespace FunStudiosWebApp.ViewModels;

/// <summary>Métricas del panel de control (RF13) + estado del lector.</summary>
public class DashboardViewModel
{
    public int NinosDentro { get; set; }

    /// <summary>Aforo de referencia mostrado como "X / 100" en la tarjeta.</summary>
    public int CapacidadMaxima { get; set; } = 100;

    public int DayPassesHoy { get; set; }
    public decimal IngresosDayPassHoy { get; set; }
    public int GratisCanjeadasTotal { get; set; }
    public int GratisDisponiblesSaldo { get; set; }

    /// <summary>Meta de checks de fidelidad leída de Parametros (para el modal de premio).</summary>
    public int ChecksParaPremio { get; set; } = 8;

    public string LectorNombre { get; set; } = string.Empty;
    public bool LectorConectado { get; set; }

    public List<MovimientoRecienteViewModel> EscaneosRecientes { get; set; } = new();
}

/// <summary>Un escaneo reciente para la lista "Escaneos Recientes".</summary>
public class MovimientoRecienteViewModel
{
    public int EntradaId { get; set; }
    public int ClienteId { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public string Inicial { get; set; } = "?";
    /// <summary>"Membresía", "Pase de Día", "Fidelidad" o "Salida".</summary>
    public string EtiquetaMovimiento { get; set; } = string.Empty;
    public DateTime Hora { get; set; }
    public bool EsEntrada { get; set; }
}

/// <summary>Pantalla principal de Reportes: bitácora + indicadores BI (RF12, RF13).</summary>
public class BitacoraViewModel
{
    public DateOnly? FechaInicio { get; set; }
    public DateOnly? FechaFin { get; set; }
    public int? TipoAccesoId { get; set; }
    public string? BusquedaCliente { get; set; }

    public List<EntradaItemViewModel> Entradas { get; set; } = new();
    public List<TipoAccesoOpcionViewModel> TiposAcceso { get; set; } = new();
}

/// <summary>Opción de filtro por tipo de acceso.</summary>
public class TipoAccesoOpcionViewModel
{
    public int TipoAccesoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
}

/// <summary>Vista de ventas / histórico básico de facturas.</summary>
public class VentasViewModel
{
    public DateOnly? FechaInicio { get; set; }
    public DateOnly? FechaFin { get; set; }

    public List<VentaItemViewModel> Ventas { get; set; } = new();
    public int TotalVentas { get; set; }
    public decimal MontoTotal { get; set; }
}

/// <summary>Serie diaria de indicadores para las gráficas BI.</summary>
public class IndicadorDiarioViewModel
{
    public DateOnly Fecha { get; set; }
    public int TotalEntradas { get; set; }
    public int DayPassesCobrados { get; set; }
    public decimal IngresosDayPass { get; set; }
    public int EntradasGratisCanjeadas { get; set; }
}

/// <summary>Tarjeta resumen del periodo analizado.</summary>
public class ResumenPeriodoBIViewModel
{
    public int DiasAnalizados { get; set; }
    public int TotalEntradas { get; set; }
    public int TotalDayPasses { get; set; }
    public decimal TotalIngresos { get; set; }
    public int TotalGratisCanjeadas { get; set; }
    public decimal PromedioIngresoDiario => DiasAnalizados == 0 ? 0 : Math.Round(TotalIngresos / DiasAnalizados, 2);
}

/// <summary>Flujo de visitantes agrupado por hora del día.</summary>
public class FlujoHoraViewModel
{
    public int Hora { get; set; }
    public int CantidadEntradas { get; set; }
}

/// <summary>Cliente con más visitas en el periodo (inteligencia de negocio básica).</summary>
public class ClienteFrecuenteViewModel
{
    public int ClienteId { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public string ApellidosFamilia { get; set; } = string.Empty;
    public int TotalVisitas { get; set; }
    public byte ConteoChecks { get; set; }
}

/// <summary>Modelo completo de la vista Reportes &gt; Inteligencia de Negocio.</summary>
public class InteligenciaNegocioViewModel
{
    public ResumenPeriodoBIViewModel Resumen { get; set; } = new();
    public List<IndicadorDiarioViewModel> SerieDiaria { get; set; } = new();
    public List<FlujoHoraViewModel> FlujoPorHoraHoy { get; set; } = new();
    public List<ClienteFrecuenteViewModel> ClientesFrecuentes { get; set; } = new();
    public int DiasAnalizados { get; set; } = 14;

    /// <summary>Meta de checks leída de Parametros (estrellas de los clientes frecuentes).</summary>
    public int ChecksParaPremio { get; set; } = 8;
}

/// <summary>Fila del listado de logs biométricos.</summary>
public class LogBiometricoItemViewModel
{
    public int LogId { get; set; }
    public DateTime FechaHoraEvento { get; set; }
    public string? Gravedad { get; set; }
    public string? Operacion { get; set; }
    public string MensajeDetalle { get; set; } = string.Empty;
    public string IPEstacionTrabajo { get; set; } = string.Empty;
    public string? UsuarioNombre { get; set; }
    public string? ClienteNombre { get; set; }
}

/// <summary>Modelo completo de la vista Reportes &gt; Logs Biométricos.</summary>
public class LogsBiometricoViewModel
{
    public List<LogBiometricoItemViewModel> Logs { get; set; } = new();
    public string? GravedadFiltro { get; set; }
}
