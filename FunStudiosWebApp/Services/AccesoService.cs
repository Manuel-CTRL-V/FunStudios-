using FunStudiosWebApp.Data;
using FunStudiosWebApp.Models;
using FunStudiosWebApp.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace FunStudiosWebApp.Services;

/// <summary>
/// Implementación del checkpoint. Aquí vive la lógica de fidelidad:
/// - RF8: cada visita Day Pass suma 1 check (máximo 8).
/// - RF9: máximo un check por día aunque salga y vuelva a entrar.
/// - RF10: al llegar a 8 checks se acredita entrada gratis y el ciclo reinicia.
/// </summary>
public class AccesoService : IAccesoService
{
    private readonly FunStudiosContext _context;
    private readonly IBiometriaService _biometria;

    /// <summary>Precio de referencia del Day Pass mientras no exista módulo de precios.</summary>
    public const decimal PrecioDayPass = 400.00m;

    public AccesoService(FunStudiosContext context, IBiometriaService biometria)
    {
        _context = context;
        _biometria = biometria;
    }

    public async Task<ResultadoEscaneoViewModel> EscanearAsync(int? usuarioId, string ip)
    {
        var resultado = new ResultadoEscaneoViewModel();

        // Clientes con huella del niño enrolada
        var clientes = await _context.Clientes
            .Include(c => c.Familia)
            .Include(c => c.FidelidadCuenta)
            .Where(c => c.HuellaNinoTemplate != null)
            .ToListAsync();

        // Familias con huella del tutor enrolada (RF4: niños menores de 2 años)
        var familiasConTutor = await _context.Familias
            .Include(f => f.Clientes)
            .Where(f => f.HuellaTutorTemplate != null)
            .ToListAsync();

        // Construye la lista ordenada de templates para la identificación 1:N,
        // guardando junto a cada template la resolución del cliente identificado.
        var candidatos = new List<(string TemplateBase64, Func<Cliente> Resolver)>();

        // Huellas de los niños enrolados
        foreach (var cliente in clientes)
        {
            var clienteId = cliente.ClienteId;
            candidatos.Add((Convert.ToBase64String(cliente.HuellaNinoTemplate!),
                () => clientes.First(c => c.ClienteId == clienteId)));
        }

        // Huellas de los tutores (RF4: niños menores de 2 años)
        foreach (var familia in familiasConTutor)
        {
            candidatos.Add((Convert.ToBase64String(familia.HuellaTutorTemplate!), () =>
            {
                // Se resuelve al niño de esa familia que dependa de la huella del tutor,
                // o al menor si todos tienen huella propia.
                return familia.Clientes
                    .OrderBy(c => c.FechaNacimiento)
                    .FirstOrDefault(c => c.HuellaNinoTemplate == null)
                    ?? familia.Clientes.OrderBy(c => c.FechaNacimiento).FirstOrDefault()
                    ?? throw new InvalidOperationException("Ningún cliente de la familia tiene huella enrolada.");
            }));
        }

        if (candidatos.Count == 0)
        {
            resultado.Mensaje = "No hay huellas enroladas en el sistema.";
            return resultado;
        }

        // Identificación 1:N real: el lector captura el dedo que el usuario coloca y
        // se compara contra las huellas enroladas en la base (RF5).
        Cliente? identificado = null;

        var indice = await _biometria.IdentificarAsync(candidatos.Select(c => c.TemplateBase64).ToList());

        if (indice >= 0 && indice < candidatos.Count)
        {
            identificado = candidatos[indice].Resolver();
        }

        if (identificado == null)
        {
            resultado.Mensaje = "Huella no reconocida. Verifique la inscripción del niño.";
            await _biometria.RegistrarEventoAsync("Identificacion", "Huella no reconocida en el checkpoint.",
                "Advertencia", usuarioId, ip: ip);
            return resultado;
        }

        await _biometria.RegistrarEventoAsync("Identificacion",
            $"Identificación exitosa: {identificado.Nombre} ({identificado.Familia!.ApellidosFamilia}).",
            "Info", usuarioId, identificado.ClienteId, ip);

        return await ConstruirResultadoAsync(identificado);
    }

    /// <summary>Arma el estado completo del cliente para la pantalla del checkpoint.</summary>
    private async Task<ResultadoEscaneoViewModel> ConstruirResultadoAsync(Cliente cliente)
    {
        var hoy = DateTime.Now.Date;

        var entradaAbierta = await _context.EntradasHistorial
            .Where(eh => eh.ClienteId == cliente.ClienteId && eh.FechaHoraSalida == null)
            .OrderByDescending(eh => eh.FechaHoraEntrada)
            .FirstOrDefaultAsync();

        var yaCobroCheckHoy = await _context.EntradasHistorial
            .AnyAsync(eh => eh.ClienteId == cliente.ClienteId &&
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
            Mensaje = $"{cliente.Nombre} identificado correctamente.",
            ClienteId = cliente.ClienteId,
            Nombre = cliente.Nombre,
            ApellidosFamilia = cliente.Familia?.ApellidosFamilia,
            Edad = cliente.Edad,
            Inicial = cliente.Inicial,
            NotasMedicas = cliente.NotasMedicas,
            EstaDentro = entradaAbierta != null,
            EntradaAbiertaId = entradaAbierta?.EntradaId,
            HoraEntradaActual = entradaAbierta?.FechaHoraEntrada,
            ConteoChecks = fidelidad.ConteoChecks,
            ChecksRestantes = fidelidad.ChecksRestantes,
            EntradasGratisDisponibles = fidelidad.EntradasGratisGanadas,
            YaCobroCheckHoy = yaCobroCheckHoy,
            MembresiaVigente = membresiaVigente != null,
            NombreMembresiaVigente = membresiaVigente?.NombreMembresia,
            VenceMembresia = membresiaVigente?.FechaFin,
            NombreTutor1 = cliente.Familia?.NombreTutor1,
            TelefonoTutor1 = cliente.Familia?.TelefonoTutor1,
            NombreTutor2 = cliente.Familia?.NombreTutor2,
            TelefonoTutor2 = cliente.Familia?.TelefonoTutor2
        };

        // Opciones de acceso disponibles cuando está fuera del parque
        if (!resultado.EstaDentro)
        {
            resultado.OpcionesEntrada.Add(new OpcionAccesoViewModel
            {
                TipoAccesoId = TipoAcceso.DayPass,
                Nombre = "Day Pass",
                Descripcion = $"Pago de entrada (${PrecioDayPass:0})",
                RequierePago = true
            });

            if (fidelidad.EntradasGratisGanadas > 0)
            {
                resultado.OpcionesEntrada.Add(new OpcionAccesoViewModel
                {
                    TipoAccesoId = TipoAcceso.FidelidadGratis,
                    Nombre = "Entrada Gratis",
                    Descripcion = $"Canjea su recompensa ({fidelidad.EntradasGratisGanadas} disponible)",
                    RequierePago = false
                });
            }

            if (membresiaVigente != null)
            {
                resultado.OpcionesEntrada.Add(new OpcionAccesoViewModel
                {
                    TipoAccesoId = TipoAcceso.Membresia,
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
            .Include(c => c.FidelidadCuenta)
            .Include(c => c.Membresias)
            .FirstOrDefaultAsync(c => c.ClienteId == solicitud.ClienteId);

        if (cliente == null)
        {
            resultado.Mensaje = "Cliente no encontrado.";
            return resultado;
        }

        resultado.NombreCliente = cliente.Nombre;

        // Control de presencia: no puede entrar si ya está dentro
        var entradaAbierta = await _context.EntradasHistorial
            .AnyAsync(eh => eh.ClienteId == cliente.ClienteId && eh.FechaHoraSalida == null);

        if (entradaAbierta)
        {
            resultado.Mensaje = $"{cliente.Nombre} ya se encuentra dentro del parque. Registre primero su salida.";
            return resultado;
        }

        // ---------- Validaciones por tipo de acceso ----------
        Venta? venta = null;

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
                    Monto = solicitud.Monto > 0 ? solicitud.Monto : PrecioDayPass,
                    Descripcion = "Day Pass",
                    FechaVenta = DateTime.Now
                };
                _context.Ventas.Add(venta);
                break;

            case TipoAcceso.FidelidadGratis:
                if ((cliente.FidelidadCuenta?.EntradasGratisGanadas ?? 0) <= 0)
                {
                    resultado.Mensaje = $"{cliente.Nombre} no tiene entradas gratis disponibles para canjear.";
                    return resultado;
                }
                break;

            case TipoAcceso.Membresia:
                if (!cliente.Membresias.Any(m => m.Vigente))
                {
                    resultado.Mensaje = $"{cliente.Nombre} no cuenta con una membresía vigente.";
                    return resultado;
                }
                break;

            default:
                resultado.Mensaje = "Tipo de acceso no válido.";
                return resultado;
        }

        await _context.SaveChangesAsync();

        // ---------- Registro de la entrada (RF6): tutor responsable ----------
        var seleccion = solicitud.SeleccionTutor ?? "tutor1";
        string? tutorNombre;
        string? tutorTelefono;

        if (seleccion == "tutor2" && !string.IsNullOrWhiteSpace(cliente.Familia?.TelefonoTutor2))
        {
            tutorNombre = string.IsNullOrWhiteSpace(cliente.Familia?.NombreTutor2) ? "Tutor 2" : cliente.Familia!.NombreTutor2!.Trim();
            tutorTelefono = cliente.Familia!.TelefonoTutor2!.Trim();
        }
        else if (seleccion == "otro")
        {
            if (string.IsNullOrWhiteSpace(solicitud.TutorOtroNombre))
            {
                resultado.Mensaje = "Al seleccionar «Otro acompañante», ingrese el nombre.";
                return resultado;
            }
            tutorNombre = solicitud.TutorOtroNombre.Trim();
            tutorTelefono = string.IsNullOrWhiteSpace(solicitud.TutorOtroTelefono) ? null : solicitud.TutorOtroTelefono.Trim();
        }
        else
        {
            tutorNombre = string.IsNullOrWhiteSpace(cliente.Familia?.NombreTutor1) ? "Tutor 1" : cliente.Familia!.NombreTutor1!.Trim();
            tutorTelefono = cliente.Familia?.TelefonoTutor1?.Trim();
        }

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
            TutorTelefono = tutorTelefono
        };

        _context.EntradasHistorial.Add(entrada);

        // Fidelidad (RF8, RF9, RF10): solo Day Pass acumula checks
        if (solicitud.TipoAccesoId == TipoAcceso.DayPass)
        {
            var fidelidad = cliente.FidelidadCuenta ??= new FidelidadCuenta { ClienteId = cliente.ClienteId };
            var hoy = DateTime.Now.Date;

            // RF9: ya existe una entrada Day Pass hoy -> no duplica check
            var yaCobroCheckHoy = await _context.EntradasHistorial
                .AnyAsync(eh => eh.ClienteId == cliente.ClienteId &&
                                eh.TipoAccesoId == TipoAcceso.DayPass &&
                                eh.FechaHoraEntrada >= hoy &&
                                eh.EntradaId != entrada.EntradaId);

            if (!yaCobroCheckHoy)
            {
                fidelidad.ConteoChecks++;
                fidelidad.FechaUltimaActualizacion = DateTime.Now;

                // RF10: al completar 8 checks se acredita la entrada gratis y reinicia el ciclo
                if (fidelidad.ConteoChecks >= FidelidadCuenta.ChecksParaPremio)
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
            ? $"¡{cliente.Nombre} completó sus 8 visitas y ganó una entrada gratis!"
            : $"Entrada registrada para {cliente.Nombre} a las {entrada.FechaHoraEntrada:HH:mm}.";

        return resultado;
    }

    public async Task<ResultadoRegistroEntradaViewModel> RegistrarSalidaAsync(int clienteId, string ip)
    {
        var resultado = new ResultadoRegistroEntradaViewModel();

        var cliente = await _context.Clientes.FindAsync(clienteId);
        if (cliente == null)
        {
            resultado.Mensaje = "Cliente no encontrado.";
            return resultado;
        }

        resultado.NombreCliente = cliente.Nombre;

        var entradaAbierta = await _context.EntradasHistorial
            .Where(eh => eh.ClienteId == clienteId && eh.FechaHoraSalida == null)
            .OrderByDescending(eh => eh.FechaHoraEntrada)
            .FirstOrDefaultAsync();

        if (entradaAbierta == null)
        {
            resultado.Mensaje = $"{cliente.Nombre} no tiene una entrada activa dentro del parque.";
            return resultado;
        }

        // RF7: hora exacta de salida para el control de presencia
        entradaAbierta.FechaHoraSalida = DateTime.Now;
        await _context.SaveChangesAsync();

        resultado.Exito = true;
        resultado.Mensaje = $"Salida registrada para {cliente.Nombre} a las {entradaAbierta.FechaHoraSalida:HH:mm}.";

        return resultado;
    }
}
