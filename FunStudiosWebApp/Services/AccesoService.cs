using FunStudiosWebApp.Data;
using FunStudiosWebApp.Models;
using FunStudiosWebApp.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace FunStudiosWebApp.Services;

/// <summary>
/// Implementación del checkpoint. Aquí vive la lógica de fidelidad:
/// - RF8: cada visita Day Pass suma 1 check (máximo checksParaPremio, configurable).
/// - RF9: máximo checksPorDia por día (configurable) aunque salga y vuelva a entrar.
/// - RF10: al llegar a la meta se acredita entrada gratis y el ciclo reinicia.
/// </summary>
public class AccesoService : IAccesoService
{
    private readonly FunStudiosContext _context;
    private readonly IConfiguracionService _configuracion;

    public AccesoService(FunStudiosContext context, IConfiguracionService configuracion)
    {
        _context = context;
        _configuracion = configuracion;
    }

    /// <summary>Arma el estado completo del cliente para la pantalla del checkpoint.</summary>
    public async Task<ResultadoEscaneoViewModel> ObtenerResultadoClienteAsync(Cliente cliente)
    {
        return await ConstruirResultadoAsync(cliente);
    }

    /// <summary>Arma el estado completo del cliente para la pantalla del checkpoint.</summary>
    private async Task<ResultadoEscaneoViewModel> ConstruirResultadoAsync(Cliente cliente)
    {
        var hoy = DateTime.Now.Date;

        var checksParaPremio = await _configuracion.ObtenerChecksParaPremioAsync();
        var tiposAcceso = await _context.TiposAcceso.ToListAsync();
        var tipoDayPass = tiposAcceso.FirstOrDefault(ta => ta.TipoAccesoId == TipoAcceso.DayPass);
        var tipoFidelidad = tiposAcceso.FirstOrDefault(ta => ta.TipoAccesoId == TipoAcceso.FidelidadGratis);
        var tipoMembresia = tiposAcceso.FirstOrDefault(ta => ta.TipoAccesoId == TipoAcceso.Membresia);

        var entradaAbierta = await _context.EntradasHistorial
            .Where(eh => eh.ClienteId == cliente.ClienteId && eh.FechaHoraSalida == null)
            .OrderByDescending(eh => eh.FechaHoraEntrada)
            .FirstOrDefaultAsync();

        var checksPorDia = await _configuracion.ObtenerChecksPorDiaAsync();
        var checksSumadosHoy = await _context.EntradasHistorial
            .CountAsync(eh => eh.ClienteId == cliente.ClienteId &&
                              eh.TipoAccesoId == TipoAcceso.DayPass &&
                              eh.FechaHoraEntrada >= hoy);

        var membresiaVigente = cliente.Membresias
            .Where(m => m.Vigente)
            .OrderByDescending(m => m.FechaFin)
            .FirstOrDefault();

        var fidelidad = cliente.FidelidadCuenta ?? new FidelidadCuenta { ConteoChecks = 0 };

        var resultado = new ResultadoEscaneoViewModel
        {
            Encontrado = true,
            Mensaje = $"{cliente.NombreMostrar} identificado correctamente.",
            ClienteId = cliente.ClienteId,
            Nombre = cliente.Nombre,
            ApellidosFamilia = cliente.Familia?.ApellidosFamilia,
            Edad = cliente.Edad,
            TieneFechaNacimiento = !cliente.SinFechaNacimiento,
            Inicial = cliente.Inicial,
            NotasMedicas = cliente.NotasMedicas,
            EstaDentro = entradaAbierta != null,
            EntradaAbiertaId = entradaAbierta?.EntradaId,
            HoraEntradaActual = entradaAbierta?.FechaHoraEntrada,
            ConteoChecks = fidelidad.ConteoChecks,
            ChecksRestantes = Math.Max(0, checksParaPremio - fidelidad.ConteoChecks),
            ChecksParaPremio = checksParaPremio,
            EntradasGratisDisponibles = fidelidad.EntradasGratisGanadas,
            ChecksSumadosHoy = checksSumadosHoy,
            ChecksPorDia = checksPorDia,
            MembresiaVigente = membresiaVigente != null,
            NombreMembresiaVigente = membresiaVigente?.NombreMembresia,
            VenceMembresia = membresiaVigente?.FechaFin,
            Tutores = (cliente.Familia?.Tutores ?? new List<Tutor>())
                .OrderByDescending(t => t.EsPrincipal)
                .ThenBy(t => t.TutorId)
                .Select(t => new TutorViewModel
                {
                    TutorId = t.TutorId,
                    Nombre = t.Nombre,
                    Telefono = t.Telefono,
                    EsPrincipal = t.EsPrincipal
                })
                .ToList()
        };

        // Opciones de acceso disponibles cuando está fuera del parque (solo tipos activos)
        if (!resultado.EstaDentro)
        {
            if (tipoDayPass is { Activo: true })
            {
                resultado.OpcionesEntrada.Add(new OpcionAccesoViewModel
                {
                    TipoAccesoId = tipoDayPass.TipoAccesoId,
                    Nombre = tipoDayPass.NombreAcceso,
                    Descripcion = string.IsNullOrWhiteSpace(tipoDayPass.Descripcion)
                        ? $"Pago de entrada (${tipoDayPass.Precio:0})"
                        : tipoDayPass.Descripcion,
                    RequierePago = true
                });
            }

            if (fidelidad.EntradasGratisGanadas > 0 && tipoFidelidad is { Activo: true })
            {
                resultado.OpcionesEntrada.Add(new OpcionAccesoViewModel
                {
                    TipoAccesoId = tipoFidelidad.TipoAccesoId,
                    Nombre = "Entrada Gratis",
                    Descripcion = $"Canjea su recompensa ({fidelidad.EntradasGratisGanadas} disponible)",
                    RequierePago = false
                });
            }

            if (membresiaVigente != null && tipoMembresia is { Activo: true })
            {
                resultado.OpcionesEntrada.Add(new OpcionAccesoViewModel
                {
                    TipoAccesoId = tipoMembresia.TipoAccesoId,
                    Nombre = membresiaVigente.NombreMembresia,
                    Descripcion = $"Membresía vigente hasta {membresiaVigente.FechaFin:dd/MM/yyyy}",
                    RequierePago = false
                });
            }
        }

        return resultado;
    }

    public async Task<ResultadoRegistroEntradaViewModel> RegistrarEntradaAsync(
        RegistrarEntradaRequest solicitud, int? usuarioId, string ip)
    {
        var resultado = new ResultadoRegistroEntradaViewModel();

        var cliente = await _context.Clientes
            .Include(c => c.Familia)
                .ThenInclude(f => f!.Tutores)
            .Include(c => c.FidelidadCuenta)
            .Include(c => c.Membresias)
            .FirstOrDefaultAsync(c => c.ClienteId == solicitud.ClienteId);

        if (cliente == null)
        {
            resultado.Mensaje = "Cliente no encontrado.";
            return resultado;
        }

        resultado.NombreCliente = cliente.NombreMostrar;

        // Control de presencia: no puede entrar si ya está dentro
        var entradaAbierta = await _context.EntradasHistorial
            .AnyAsync(eh => eh.ClienteId == cliente.ClienteId && eh.FechaHoraSalida == null);

        if (entradaAbierta)
        {
            resultado.Mensaje = $"{cliente.NombreMostrar} ya se encuentra dentro del parque. Registre primero su salida.";
            return resultado;
        }

        // ---------- Validaciones por tipo de acceso ----------
        Venta? venta = null;

        var tiposAcceso = await _context.TiposAcceso.ToListAsync();
        var tipoSeleccionado = tiposAcceso.FirstOrDefault(ta => ta.TipoAccesoId == solicitud.TipoAccesoId);

        if (tipoSeleccionado == null)
        {
            resultado.Mensaje = "Tipo de acceso no válido.";
            return resultado;
        }

        // Un tipo desactivado desde Configuración no permite registrar entradas
        var tipoActivo = tiposAcceso.FirstOrDefault(ta => ta.TipoAccesoId == solicitud.TipoAccesoId && ta.Activo);
        if (tipoActivo == null)
        {
            resultado.Mensaje = $"El acceso '{tipoSeleccionado.NombreAcceso}' está desactivado. Dígale al administrador para reactivarlo.";
            return resultado;
        }

        switch (solicitud.TipoAccesoId)
        {
            case TipoAcceso.DayPass:
                if (solicitud.MetodoPagoId == null)
                {
                    resultado.Mensaje = "Seleccione el método de pago para el Day Pass.";
                    return resultado;
                }

                venta = new Venta
                {
                    ClienteId = cliente.ClienteId,
                    MetodoPagoId = solicitud.MetodoPagoId.Value,
                    Monto = solicitud.Monto > 0 ? solicitud.Monto : tipoActivo.Precio,
                    Descripcion = tipoActivo.NombreAcceso,
                    FechaVenta = DateTime.Now
                };
                _context.Ventas.Add(venta);
                break;

            case TipoAcceso.FidelidadGratis:
                if ((cliente.FidelidadCuenta?.EntradasGratisGanadas ?? 0) <= 0)
                {
                    resultado.Mensaje = $"{cliente.NombreMostrar} no tiene entradas gratis disponibles para canjear.";
                    return resultado;
                }
                break;

            case TipoAcceso.Membresia:
                if (!cliente.Membresias.Any(m => m.Vigente))
                {
                    resultado.Mensaje = $"{cliente.NombreMostrar} no cuenta con una membresía vigente.";
                    return resultado;
                }
                break;

            default:
                resultado.Mensaje = "Tipo de acceso no válido.";
                return resultado;
        }

        await _context.SaveChangesAsync();

        // ---------- Registro de la entrada (RF6): tutor(es) responsable(s) ----------
        var tutoresSeleccionados = (cliente.Familia?.Tutores ?? new List<Tutor>())
            .Where(t => solicitud.TutorIds.Contains(t.TutorId))
            .OrderBy(t => t.TutorId)
            .ToList();

        var nombresTutores = tutoresSeleccionados.Select(t => t.Nombre.Trim()).ToList();

        // Acompañante no registrado (opcional, se anexa al snapshot de responsables)
        if (!string.IsNullOrWhiteSpace(solicitud.TutorOtroNombre))
        {
            nombresTutores.Add(solicitud.TutorOtroNombre.Trim());
        }

        // Si no se seleccionó ninguno, se usa el tutor principal de la familia como responsable
        if (nombresTutores.Count == 0)
        {
            var principal = (cliente.Familia?.Tutores ?? new List<Tutor>())
                .FirstOrDefault(t => t.EsPrincipal);
            if (principal != null)
            {
                tutoresSeleccionados.Add(principal);
                nombresTutores.Add(principal.Nombre.Trim());
            }
        }

        var tutorNombre = nombresTutores.Count > 0 ? string.Join(", ", nombresTutores) : null;
        var tutorTelefono = tutoresSeleccionados.FirstOrDefault()?.Telefono
            ?? (!string.IsNullOrWhiteSpace(solicitud.TutorOtroTelefono) ? solicitud.TutorOtroTelefono.Trim() : null);

        // La cantidad de acompañantes incluye los tutores seleccionados y no puede ser menor
        var acompanantes = Math.Max(solicitud.Acompanantes, tutoresSeleccionados.Count);

        var entrada = new EntradaHistorial
        {
            FamiliaId = cliente.FamiliaId,
            ClienteId = cliente.ClienteId,
            FechaHoraEntrada = DateTime.Now,
            TipoAccesoId = solicitud.TipoAccesoId,
            MembresiaId = solicitud.TipoAccesoId == TipoAcceso.Membresia
                ? cliente.Membresias.Where(m => m.Vigente).OrderByDescending(m => m.FechaFin).First().MembresiaId
                : null,
            VentaId = venta?.VentaId,
            TutorNombre = tutorNombre,
            TutorTelefono = tutorTelefono,
            Acompanantes = acompanantes
        };

        _context.EntradasHistorial.Add(entrada);

        // Canjear una entrada gratis consume la recompensa acumulada (RF10)
        if (solicitud.TipoAccesoId == TipoAcceso.FidelidadGratis)
        {
            cliente.FidelidadCuenta!.EntradasGratisGanadas--;
            cliente.FidelidadCuenta!.FechaUltimaActualizacion = DateTime.Now;
        }

        // Fidelidad (RF8, RF9, RF10): solo Day Pass acumula checks
        var checksParaPremio = await _configuracion.ObtenerChecksParaPremioAsync();

        if (solicitud.TipoAccesoId == TipoAcceso.DayPass)
        {
            var fidelidad = cliente.FidelidadCuenta ??= new FidelidadCuenta { ClienteId = cliente.ClienteId };
            var hoy = DateTime.Now.Date;

            // RF9: límite de checks por día configurable (Parámetro "Fidelidad.ChecksPorDia").
            // La entrada actual aún no está persistida, así que el conteo de hoy no la incluye.
            var checksPorDia = await _configuracion.ObtenerChecksPorDiaAsync();
            var checksSumadosHoy = await _context.EntradasHistorial
                .CountAsync(eh => eh.ClienteId == cliente.ClienteId &&
                                  eh.TipoAccesoId == TipoAcceso.DayPass &&
                                  eh.FechaHoraEntrada >= hoy);

            if (checksSumadosHoy < checksPorDia)
            {
                fidelidad.ConteoChecks++;
                fidelidad.FechaUltimaActualizacion = DateTime.Now;

                // RF10: al completar la meta configurada se acredita la entrada gratis y reinicia el ciclo
                if (fidelidad.ConteoChecks >= checksParaPremio)
                {
                    fidelidad.ConteoChecks = 0;
                    fidelidad.EntradasGratisGanadas++;
                    resultado.PremioGanado = true;
                }
            }

            resultado.ConteoChecks = fidelidad.ConteoChecks;
        }

        await _context.SaveChangesAsync();

        resultado.Exito = true;
        resultado.Mensaje = resultado.PremioGanado
            ? $"¡{cliente.NombreMostrar} completó sus {checksParaPremio} visitas y ganó una entrada gratis!"
            : $"Entrada registrada para {cliente.NombreMostrar} a las {entrada.FechaHoraEntrada:HH:mm}.";

        return resultado;
    }

    public async Task<ResultadoRegistroEntradaViewModel> RegistrarSalidaAsync(int clienteId, string ip, int? usuarioId = null)
    {
        var resultado = new ResultadoRegistroEntradaViewModel();

        var cliente = await _context.Clientes.FindAsync(clienteId);
        if (cliente == null)
        {
            resultado.Mensaje = "Cliente no encontrado.";
            return resultado;
        }

        resultado.NombreCliente = cliente.NombreMostrar;

        var entradaAbierta = await _context.EntradasHistorial
            .Where(eh => eh.ClienteId == clienteId && eh.FechaHoraSalida == null)
            .OrderByDescending(eh => eh.FechaHoraEntrada)
            .FirstOrDefaultAsync();

        if (entradaAbierta == null)
        {
            resultado.Mensaje = $"{cliente.NombreMostrar} no tiene una entrada activa dentro del parque.";
            return resultado;
        }

        // RF7: hora exacta de salida para el control de presencia
        entradaAbierta.FechaHoraSalida = DateTime.Now;
        await _context.SaveChangesAsync();

        resultado.Exito = true;
        resultado.Mensaje = $"Salida registrada para {cliente.NombreMostrar} a las {entradaAbierta.FechaHoraSalida:HH:mm}.";

        return resultado;
    }
}
