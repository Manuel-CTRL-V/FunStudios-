using FunStudiosWebApp.Data;
using FunStudiosWebApp.Models;
using FunStudiosWebApp.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace FunStudiosWebApp.Services;

/// <summary>Implementación de reportes, bitácora y métricas (RF12, RF13).</summary>
public class ReporteService : IReporteService
{
    private readonly FunStudiosContext _context;
    private readonly IBiometriaService _biometria;
    private readonly IConfiguracionService _configuracion;

    public ReporteService(FunStudiosContext context, IBiometriaService biometria, IConfiguracionService configuracion)
    {
        _context = context;
        _biometria = biometria;
        _configuracion = configuracion;
    }

    public async Task<DashboardViewModel> ObtenerDashboardAsync()
    {
        var hoy = DateTime.Now.Date;

        var ninosDentroTask = _context.EntradasHistorial
            .CountAsync(eh => eh.FechaHoraSalida == null);

        var dayPassesHoyTask = _context.EntradasHistorial
            .CountAsync(eh => eh.TipoAccesoId == TipoAcceso.DayPass && eh.FechaHoraEntrada >= hoy);

        var ingresosDayPassHoyTask = _context.Ventas
            .Where(v => v.Descripcion == "Day Pass" && v.FechaVenta >= hoy)
            .SumAsync(v => (decimal?)v.Monto);

        var gratisCanjeadasTask = _context.EntradasHistorial
            .CountAsync(eh => eh.TipoAccesoId == TipoAcceso.FidelidadGratis);

        var gratisSaldoTask = _context.FidelidadCuentas
            .SumAsync(fc => fc.EntradasGratisGanadas);

        var escaneosRecientesTask = ObtenerMovimientosRecientesAsync(6);
        var checksParaPremioTask = _configuracion.ObtenerChecksParaPremioAsync();

        await Task.WhenAll(ninosDentroTask, dayPassesHoyTask, ingresosDayPassHoyTask,
            gratisCanjeadasTask, gratisSaldoTask, escaneosRecientesTask, checksParaPremioTask);

        return new DashboardViewModel
        {
            NinosDentro = await ninosDentroTask,
            DayPassesHoy = await dayPassesHoyTask,
            IngresosDayPassHoy = await ingresosDayPassHoyTask ?? 0m,
            GratisCanjeadasTotal = await gratisCanjeadasTask,
            GratisDisponiblesSaldo = await gratisSaldoTask,
            ChecksParaPremio = await checksParaPremioTask,
            LectorNombre = "Lector biométrico",
            LectorConectado = false,
            EscaneosRecientes = await escaneosRecientesTask
        };
    }

    /// <summary>Últimos movimientos de hoy para la lista "Escaneos Recientes" del checkpoint.</summary>
    private async Task<List<MovimientoRecienteViewModel>> ObtenerMovimientosRecientesAsync(int cantidad)
    {
        var entradasTask = _context.EntradasHistorial
            .Include(eh => eh.Cliente)
            .Include(eh => eh.TipoAcceso)
            .OrderByDescending(eh => eh.FechaHoraEntrada)
            .Take(cantidad)
            .ToListAsync();

        var salidasTask = _context.EntradasHistorial
            .Where(eh => eh.FechaHoraSalida != null)
            .Include(eh => eh.Cliente)
            .OrderByDescending(eh => eh.FechaHoraSalida)
            .Take(cantidad)
            .ToListAsync();

        await Task.WhenAll(entradasTask, salidasTask);

        var entradas = await entradasTask;
        var salidas = await salidasTask;

        return entradas.Select(eh => new MovimientoRecienteViewModel
        {
            EntradaId = eh.EntradaId,
            ClienteId = eh.ClienteId,
            NombreCliente = eh.Cliente!.Nombre,
            Inicial = eh.Cliente.Inicial,
            EtiquetaMovimiento = eh.TipoAcceso?.NombreAcceso ?? string.Empty,
            Hora = eh.FechaHoraEntrada,
            EsEntrada = true
        })
        .Concat(salidas.Select(eh => new MovimientoRecienteViewModel
        {
            EntradaId = eh.EntradaId,
            ClienteId = eh.ClienteId,
            NombreCliente = eh.Cliente!.Nombre,
            Inicial = eh.Cliente.Inicial,
            EtiquetaMovimiento = "Salida",
            Hora = eh.FechaHoraSalida!.Value,
            EsEntrada = false
        }))
        .OrderByDescending(m => m.Hora)
        .Take(cantidad)
        .ToList();
    }

    public async Task<BitacoraViewModel> ObtenerBitacoraAsync(DateOnly? fechaInicio, DateOnly? fechaFin,
        int? tipoAccesoId, string? busquedaCliente)
    {
        var consulta = _context.EntradasHistorial
            .Include(eh => eh.Cliente)
            .Include(eh => eh.Familia)
            .Include(eh => eh.TipoAcceso)
            .Include(eh => eh.Venta)
            .AsQueryable();

        if (fechaInicio.HasValue)
        {
            var inicio = fechaInicio.Value.ToDateTime(TimeOnly.MinValue);
            consulta = consulta.Where(eh => eh.FechaHoraEntrada >= inicio);
        }

        if (fechaFin.HasValue)
        {
            var fin = fechaFin.Value.ToDateTime(new TimeOnly(23, 59, 59));
            consulta = consulta.Where(eh => eh.FechaHoraEntrada <= fin);
        }

        if (tipoAccesoId.HasValue)
            consulta = consulta.Where(eh => eh.TipoAccesoId == tipoAccesoId.Value);

        if (!string.IsNullOrWhiteSpace(busquedaCliente))
        {
            var texto = busquedaCliente.Trim().ToLower();
            consulta = consulta.Where(eh =>
                eh.Cliente!.Nombre.ToLower().Contains(texto) ||
                eh.Familia!.ApellidosFamilia.ToLower().Contains(texto));
        }

        var tiposAcceso = await _context.TiposAcceso
            .Select(ta => new TipoAccesoOpcionViewModel { TipoAccesoId = ta.TipoAccesoId, Nombre = ta.NombreAcceso })
            .ToListAsync();

        var entradas = await consulta
            .OrderByDescending(eh => eh.FechaHoraEntrada)
            .Take(200)
            .ToListAsync();

        return new BitacoraViewModel
        {
            FechaInicio = fechaInicio,
            FechaFin = fechaFin,
            TipoAccesoId = tipoAccesoId,
            BusquedaCliente = busquedaCliente,
            TiposAcceso = tiposAcceso,
            Entradas = entradas.Select(eh => new EntradaItemViewModel
            {
                EntradaId = eh.EntradaId,
                ClienteId = eh.ClienteId,
                NombreCliente = eh.Cliente?.Nombre ?? string.Empty,
                ApellidosFamilia = eh.Familia?.ApellidosFamilia ?? string.Empty,
                FechaHoraEntrada = eh.FechaHoraEntrada,
                FechaHoraSalida = eh.FechaHoraSalida,
                TipoAccesoNombre = eh.TipoAcceso?.NombreAcceso ?? string.Empty,
                TipoAccesoId = eh.TipoAccesoId,
                TutorNombre = eh.TutorNombre,
                TutorTelefono = eh.TutorTelefono,
                Acompanantes = eh.Acompanantes,
                MontoVenta = eh.Venta?.Monto
            }).ToList()
        };
    }

    public async Task<VentasViewModel> ObtenerVentasAsync(DateOnly? fechaInicio, DateOnly? fechaFin)
    {
        var consulta = _context.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.MetodoPago)
            .AsQueryable();

        if (fechaInicio.HasValue)
        {
            var inicio = fechaInicio.Value.ToDateTime(TimeOnly.MinValue);
            consulta = consulta.Where(v => v.FechaVenta >= inicio);
        }

        if (fechaFin.HasValue)
        {
            var fin = fechaFin.Value.ToDateTime(new TimeOnly(23, 59, 59));
            consulta = consulta.Where(v => v.FechaVenta <= fin);
        }

        var ventas = await consulta
            .OrderByDescending(v => v.FechaVenta)
            .Take(300)
            .ToListAsync();

        return new VentasViewModel
        {
            FechaInicio = fechaInicio,
            FechaFin = fechaFin,
            Ventas = ventas.Select(MapVenta).ToList(),
            TotalVentas = ventas.Count,
            MontoTotal = ventas.Sum(v => v.Monto)
        };
    }

    private static VentaItemViewModel MapVenta(Venta v) => new()
    {
        VentaId = v.VentaId,
        FechaVenta = v.FechaVenta,
        ClienteId = v.ClienteId,
        NombreCliente = v.Cliente?.Nombre ?? $"Cliente #{v.ClienteId}",
        MetodoPagoNombre = v.MetodoPago?.NombreMetodo ?? string.Empty,
        Monto = v.Monto,
        Descripcion = v.Descripcion
    };

    public async Task<InteligenciaNegocioViewModel> ObtenerInteligenciaNegocioAsync(int dias)
    {
        dias = Math.Clamp(dias, 7, 90);
        var inicio = DateTime.Now.Date.AddDays(-(dias - 1));

        var resumen = await CalcularResumenPeriodoAsync(inicio, dias);

        // ---------- Serie diaria ----------
        var inicioDia = DateOnly.FromDateTime(inicio);
        var finDia = DateOnly.FromDateTime(DateTime.Now.Date);

        var entradasPorDia = await _context.EntradasHistorial
            .GroupBy(eh => DateOnly.FromDateTime(eh.FechaHoraEntrada))
            .Select(g => new { Fecha = g.Key, Total = g.Count() })
            .ToDictionaryAsync(x => x.Fecha, x => x.Total);

        var dayPassesPorDia = await _context.EntradasHistorial
            .Where(eh => eh.TipoAccesoId == TipoAcceso.DayPass)
            .GroupBy(eh => DateOnly.FromDateTime(eh.FechaHoraEntrada))
            .Select(g => new { Fecha = g.Key, Total = g.Count() })
            .ToDictionaryAsync(x => x.Fecha, x => x.Total);

        var ingresosPorDia = await _context.Ventas
            .Where(v => v.Descripcion == "Day Pass")
            .GroupBy(v => DateOnly.FromDateTime(v.FechaVenta))
            .Select(g => new { Fecha = g.Key, Monto = g.Sum(v => v.Monto) })
            .ToDictionaryAsync(x => x.Fecha, x => x.Monto);

        var gratisPorDia = await _context.EntradasHistorial
            .Where(eh => eh.TipoAccesoId == TipoAcceso.FidelidadGratis)
            .GroupBy(eh => DateOnly.FromDateTime(eh.FechaHoraEntrada))
            .Select(g => new { Fecha = g.Key, Total = g.Count() })
            .ToDictionaryAsync(x => x.Fecha, x => x.Total);

        var serieDiaria = new List<IndicadorDiarioViewModel>();
        for (var dia = inicioDia; dia <= finDia; dia = dia.AddDays(1))
        {
            serieDiaria.Add(new IndicadorDiarioViewModel
            {
                Fecha = dia,
                TotalEntradas = entradasPorDia.GetValueOrDefault(dia),
                DayPassesCobrados = dayPassesPorDia.GetValueOrDefault(dia),
                IngresosDayPass = ingresosPorDia.GetValueOrDefault(dia),
                EntradasGratisCanjeadas = gratisPorDia.GetValueOrDefault(dia)
            });
        }

        // ---------- Flujo por hora de HOY ----------
        var hoy = DateTime.Now.Date;
        var horasHoy = await _context.EntradasHistorial
            .Where(eh => eh.FechaHoraEntrada >= hoy && eh.FechaHoraEntrada < hoy.AddDays(1))
            .ToListAsync();

        // FunStudios: L-V 3:00PM-8:00PM | S-D y feriados 10:00AM-9:00PM
        var esFinDeSemana = hoy.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
        var horaInicio = esFinDeSemana ? 10 : 15;
        var horasTotales = esFinDeSemana ? 11 : 6;

        var flujoPorHora = Enumerable.Range(horaInicio, horasTotales).Select(hora => new FlujoHoraViewModel
        {
            Hora = hora,
            CantidadEntradas = horasHoy.Count(eh => eh.FechaHoraEntrada.Hour == hora)
        }).ToList();

        // ---------- Clientes más frecuentes del periodo ----------
        var clientesFrecuentes = await _context.EntradasHistorial
            .Where(eh => eh.FechaHoraEntrada >= inicio)
            .GroupBy(eh => new { eh.ClienteId, eh.Cliente!.Nombre, eh.Familia!.ApellidosFamilia, Checks = eh.Cliente.FidelidadCuenta!.ConteoChecks })
            .Select(g => new ClienteFrecuenteViewModel
            {
                ClienteId = g.Key.ClienteId,
                NombreCliente = g.Key.Nombre,
                ApellidosFamilia = g.Key.ApellidosFamilia,
                TotalVisitas = g.Count(),
                ConteoChecks = g.Key.Checks
            })
            .OrderByDescending(cf => cf.TotalVisitas)
            .Take(5)
            .ToListAsync();

        return new InteligenciaNegocioViewModel
        {
            Resumen = resumen,
            SerieDiaria = serieDiaria,
            FlujoPorHoraHoy = flujoPorHora,
            ClientesFrecuentes = clientesFrecuentes,
            DiasAnalizados = dias,
            ChecksParaPremio = await _configuracion.ObtenerChecksParaPremioAsync()
        };
    }

    private async Task<ResumenPeriodoBIViewModel> CalcularResumenPeriodoAsync(DateTime inicio, int dias)
    {
        var totalEntradas = await _context.EntradasHistorial
            .CountAsync(eh => eh.FechaHoraEntrada >= inicio);

        var totalDayPasses = await _context.EntradasHistorial
            .CountAsync(eh => eh.FechaHoraEntrada >= inicio && eh.TipoAccesoId == TipoAcceso.DayPass);

        var totalIngresos = await _context.Ventas
            .Where(v => v.FechaVenta >= inicio)
            .SumAsync(v => (decimal?)v.Monto) ?? 0m;

        var totalGratis = await _context.EntradasHistorial
            .CountAsync(eh => eh.FechaHoraEntrada >= inicio && eh.TipoAccesoId == TipoAcceso.FidelidadGratis);

        return new ResumenPeriodoBIViewModel
        {
            DiasAnalizados = dias,
            TotalEntradas = totalEntradas,
            TotalDayPasses = totalDayPasses,
            TotalIngresos = totalIngresos,
            TotalGratisCanjeadas = totalGratis
        };
    }

    public async Task<List<LogBiometricoItemViewModel>> ObtenerLogsBiometricoAsync(string? gravedad)
    {
        var consulta = _context.LogsBiometrico
            .Include(lb => lb.Usuario)
            .Include(lb => lb.Cliente)
                .ThenInclude(c => c!.Familia)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(gravedad))
            consulta = consulta.Where(lb => lb.Gravedad == gravedad);

        var logs = await consulta
            .OrderByDescending(lb => lb.FechaHoraEvento)
            .Take(150)
            .ToListAsync();

        return logs.Select(lb => new LogBiometricoItemViewModel
        {
            LogId = lb.LogId,
            FechaHoraEvento = lb.FechaHoraEvento,
            Gravedad = lb.Gravedad,
            Operacion = lb.Operacion,
            MensajeDetalle = lb.MensajeDetalle,
            IPEstacionTrabajo = lb.IPEstacionTrabajo,
            UsuarioNombre = lb.Usuario?.NombreCompleto,
            ClienteNombre = lb.Cliente != null ? $"{lb.Cliente.Nombre} ({lb.Cliente.Familia?.ApellidosFamilia})" : null
        }).ToList();
    }
}
