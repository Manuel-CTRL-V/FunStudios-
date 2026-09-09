using FunStudiosWebApp.Data;
using FunStudiosWebApp.Models;
using FunStudiosWebApp.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace FunStudiosWebApp.Services;

/// <summary>Implementación de la gestión de clientes.</summary>
public class ClienteService : IClienteService
{
    private readonly FunStudiosContext _context;
    private readonly IBiometriaService _biometria;
    private readonly IConfiguracionService _configuracion;

    public ClienteService(FunStudiosContext context, IBiometriaService biometria, IConfiguracionService configuracion)
    {
        _context = context;
        _biometria = biometria;
        _configuracion = configuracion;
    }

    public async Task<List<ClienteListadoItemViewModel>> ObtenerListadoAsync(string? busqueda, string? estado)
    {
        var checksParaPremio = await _configuracion.ObtenerChecksParaPremioAsync();

        var consulta = _context.Clientes
            .Include(c => c.Familia)
            .Include(c => c.FidelidadCuenta)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var texto = busqueda.Trim().ToLower();
            consulta = consulta.Where(c =>
                c.Nombre.ToLower().Contains(texto) ||
                c.Familia!.ApellidosFamilia.ToLower().Contains(texto) ||
                (c.Familia!.CodigoFamilia != null && c.Familia!.CodigoFamilia.ToLower().Contains(texto)));
        }

        var clientes = await consulta
            .OrderBy(c => c.Nombre)
            .ToListAsync();

        // Presencia e historial se consultan en bloque para evitar N+1
        var clienteIds = clientes.Select(c => c.ClienteId).ToList();

        var entradasAbiertas = await _context.EntradasHistorial
            .Where(eh => eh.FechaHoraSalida == null && clienteIds.Contains(eh.ClienteId))
            .Select(eh => eh.ClienteId)
            .ToListAsync();

        var ultimasEntradas = await _context.EntradasHistorial
            .Where(eh => clienteIds.Contains(eh.ClienteId))
            .GroupBy(eh => eh.ClienteId)
            .Select(g => new { ClienteId = g.Key, Ultima = g.Max(eh => eh.FechaHoraEntrada) })
            .ToDictionaryAsync(x => x.ClienteId, x => (DateTime?)x.Ultima);

        return clientes.Select(c =>
        {
            var dentro = entradasAbiertas.Contains(c.ClienteId);
            return new ClienteListadoItemViewModel
            {
                ClienteId = c.ClienteId,
                Nombre = c.Nombre,
                Inicial = c.Inicial,
                Edad = c.Edad,
                ApellidosFamilia = c.Familia?.ApellidosFamilia ?? string.Empty,
                CodigoFamilia = c.Familia?.CodigoFamilia,
                Nacionalidad = c.Nacionalidad,
                TieneNotaMedica = !string.IsNullOrWhiteSpace(c.NotasMedicas),
                ConteoChecks = c.FidelidadCuenta?.ConteoChecks ?? 0,
                ChecksParaPremio = checksParaPremio,
                EntradasGratisDisponibles = c.FidelidadCuenta?.EntradasGratisGanadas ?? 0,
                EstaDentro = dentro,
                UltimaEntradaTexto = FormatearUltimaEntrada(ultimasEntradas.GetValueOrDefault(c.ClienteId))
            };
        })
        .Where(c => estado?.ToLower() switch
        {
            "dentro" => c.EstaDentro,
            "fuera" => !c.EstaDentro,
            _ => true
        })
        .ToList();
    }

    private static string FormatearUltimaEntrada(DateTime? ultimaEntrada)
    {
        if (ultimaEntrada == null) return "Sin visitas";

        if (DateOnly.FromDateTime(ultimaEntrada.Value) == DateOnly.FromDateTime(DateTime.Now))
            return $"Hoy, {ultimaEntrada.Value:h:mm tt}";

        return ultimaEntrada.Value.ToString("dd MMM, yyyy", new System.Globalization.CultureInfo("es-MX"));
    }

    public async Task<ClienteDetalleViewModel?> ObtenerDetalleAsync(int clienteId)
    {
        var checksParaPremio = await _configuracion.ObtenerChecksParaPremioAsync();

        var cliente = await _context.Clientes
            .Include(c => c.Familia)
                .ThenInclude(f => f!.Tutores)
            .Include(c => c.FidelidadCuenta)
            .FirstOrDefaultAsync(c => c.ClienteId == clienteId);

        if (cliente == null) return null;

        var estaDentro = await _context.EntradasHistorial
            .AnyAsync(eh => eh.ClienteId == clienteId && eh.FechaHoraSalida == null);

        var entradasRecientes = await _context.EntradasHistorial
            .Where(eh => eh.ClienteId == clienteId)
            .Include(eh => eh.TipoAcceso)
            .Include(eh => eh.Venta)
            .OrderByDescending(eh => eh.FechaHoraEntrada)
            .Take(10)
            .ToListAsync();

        var ventasRecientes = await _context.Ventas
            .Where(v => v.ClienteId == clienteId)
            .Include(v => v.MetodoPago)
            .OrderByDescending(v => v.FechaVenta)
            .Take(10)
            .ToListAsync();

        return new ClienteDetalleViewModel
        {
            ClienteId = cliente.ClienteId,
            Nombre = cliente.Nombre,
            Inicial = cliente.Inicial,
            FechaNacimiento = cliente.FechaNacimiento,
            Edad = cliente.Edad,
            NotasMedicas = cliente.NotasMedicas,
            Nacionalidad = cliente.Nacionalidad,
            HuellaEnrolada = cliente.HuellaNinoTemplate != null,
            HuellaTutorEnrolada = cliente.Familia?.Tutores.Any(t => t.HuellaTemplate != null) ?? false,
            FamiliaId = cliente.FamiliaId,
            ApellidosFamilia = cliente.Familia?.ApellidosFamilia ?? string.Empty,
            CodigoFamilia = cliente.Familia?.CodigoFamilia,
            Tutores = (cliente.Familia?.Tutores ?? new List<Tutor>())
                .OrderByDescending(t => t.EsPrincipal)
                .ThenBy(t => t.TutorId)
                .Select(t => new TutorViewModel
                {
                    TutorId = t.TutorId,
                    Nombre = t.Nombre,
                    Telefono = t.Telefono,
                    EsPrincipal = t.EsPrincipal,
                    HuellaEnrolada = t.HuellaTemplate != null
                })
                .ToList(),
            ConteoChecks = cliente.FidelidadCuenta?.ConteoChecks ?? 0,
            ChecksParaPremio = checksParaPremio,
            EntradasGratisDisponibles = cliente.FidelidadCuenta?.EntradasGratisGanadas ?? 0,
            FechaUltimaActualizacionFidelidad = cliente.FidelidadCuenta?.FechaUltimaActualizacion,
            EstaDentro = estaDentro,
            EntradasRecientes = entradasRecientes.Select(eh => new EntradaItemViewModel
            {
                EntradaId = eh.EntradaId,
                ClienteId = cliente.ClienteId,
                NombreCliente = cliente.Nombre,
                ApellidosFamilia = cliente.Familia?.ApellidosFamilia ?? string.Empty,
                FechaHoraEntrada = eh.FechaHoraEntrada,
                FechaHoraSalida = eh.FechaHoraSalida,
                TipoAccesoNombre = eh.TipoAcceso?.NombreAcceso ?? string.Empty,
                TipoAccesoId = eh.TipoAccesoId,
                TutorNombre = eh.TutorNombre,
                TutorTelefono = eh.TutorTelefono,
                Acompanantes = eh.Acompanantes,
                MontoVenta = eh.Venta?.Monto
            }).ToList(),
            Membresias = cliente.Membresias
                .OrderByDescending(m => m.FechaInicio)
                .Select(m => new MembresiaItemViewModel
                {
                    MembresiaId = m.MembresiaId,
                    NombreMembresia = m.NombreMembresia,
                    Descripcion = m.Descripcion,
                    FechaInicio = m.FechaInicio,
                    FechaFin = m.FechaFin,
                    Precio = m.Precio,
                    Estado = m.Estado,
                    Vigente = m.Vigente
                }).ToList(),
            VentasRecientes = ventasRecientes.Select(v => new VentaItemViewModel
            {
                VentaId = v.VentaId,
                FechaVenta = v.FechaVenta,
                ClienteId = v.ClienteId,
                NombreCliente = cliente.Nombre,
                MetodoPagoNombre = v.MetodoPago?.NombreMetodo ?? string.Empty,
                Monto = v.Monto,
                Descripcion = v.Descripcion
            }).ToList()
        };
    }

    public async Task<List<FamiliaOpcionViewModel>> ObtenerFamiliasAsync()
    {
        return await _context.Familias
            .OrderBy(f => f.ApellidosFamilia)
            .Select(f => new FamiliaOpcionViewModel
            {
                FamiliaId = f.FamiliaId,
                ApellidosFamilia = f.ApellidosFamilia,
                CodigoFamilia = f.CodigoFamilia,
                Etiqueta = string.IsNullOrWhiteSpace(f.CodigoFamilia)
                    ? $"Familia {f.ApellidosFamilia}"
                    : $"Familia {f.ApellidosFamilia} · {f.CodigoFamilia}"
            })
            .ToListAsync();
    }

    public async Task<(bool Exito, string Mensaje, int ClienteId)> RegistrarAsync(
        ClienteCrearViewModel modelo, int? usuarioId, string ip)
    {
        // ---------- Validaciones (RF1, RF2, RF3) ----------
        if (!modelo.FechaNacimiento.HasValue)
            return (false, "La fecha de nacimiento del niño es obligatoria.", 0);

        if (modelo.FechaNacimiento.Value > DateOnly.FromDateTime(DateTime.Now))
            return (false, "La fecha de nacimiento no puede ser futura.", 0);

        if (string.IsNullOrWhiteSpace(modelo.HuellaNinoTemplateBase64))
            return (false, "Complete el enrolamiento biométrico: faltan muestras de huella (se requieren 3).", 0);

        byte[] templateNino;
        try { templateNino = Convert.FromBase64String(modelo.HuellaNinoTemplateBase64); }
        catch (FormatException) { return (false, "El template biométrico recibido no es válido.", 0); }

        // Evitar enrolar la misma huella dos veces
        var duplicadoNino = await _context.Clientes.AnyAsync(c => c.HuellaNinoTemplate!.SequenceEqual(templateNino));
        if (duplicadoNino)
            return (false, "Esta huella ya está registrada en el sistema.", 0);

        // ---------- Familia nueva o existente (RF2) ----------
        Familia familia;

        if (modelo.UsarFamiliaExistente)
        {
            var familiaExistente = await _context.Familias
                .Include(f => f.Tutores)
                .FirstOrDefaultAsync(f => f.FamiliaId == (modelo.FamiliaExistenteId ?? 0));
            if (familiaExistente == null)
                return (false, "Seleccione una familia válida.", 0);
            familia = familiaExistente;
        }
        else
        {
            if (string.IsNullOrWhiteSpace(modelo.ApellidosFamilia))
                return (false, "Los apellidos de la familia son obligatorios.", 0);
            if (string.IsNullOrWhiteSpace(modelo.TelefonoTutor1))
                return (false, "El teléfono del tutor 1 es obligatorio.", 0);

            familia = new Familia
            {
                ApellidosFamilia = modelo.ApellidosFamilia.Trim(),
                CodigoFamilia = await GenerarCodigoFamiliaAsync(),
                Tutores = new List<Tutor>
                {
                    new()
                    {
                        Nombre = string.IsNullOrWhiteSpace(modelo.NombreTutor1) ? "Tutor 1" : modelo.NombreTutor1.Trim(),
                        Telefono = modelo.TelefonoTutor1.Trim(),
                        EsPrincipal = true
                    }
                }
            };

            // Segundo tutor opcional al crear la familia
            if (!string.IsNullOrWhiteSpace(modelo.TelefonoTutor2))
            {
                familia.Tutores.Add(new Tutor
                {
                    Nombre = string.IsNullOrWhiteSpace(modelo.NombreTutor2) ? "Tutor 2" : modelo.NombreTutor2.Trim(),
                    Telefono = modelo.TelefonoTutor2.Trim(),
                    EsPrincipal = false
                });
            }
            _context.Familias.Add(familia);
        }

        var cliente = new Cliente
        {
            Nombre = modelo.Nombre.Trim(),
            FechaNacimiento = modelo.FechaNacimiento.Value,
            NotasMedicas = string.IsNullOrWhiteSpace(modelo.NotasMedicas) ? null : modelo.NotasMedicas.Trim(),
            Nacionalidad = string.IsNullOrWhiteSpace(modelo.Nacionalidad) ? null : modelo.Nacionalidad.Trim(),
            HuellaNinoTemplate = templateNino,
            Familia = familia
        };

        _context.Clientes.Add(cliente);

        // Cuenta de fidelidad inicial (relación 1 a 1)
        cliente.FidelidadCuenta = new FidelidadCuenta
        {
            ConteoChecks = 0,
            EntradasGratisGanadas = 0
        };

        await _context.SaveChangesAsync();

        // ---------- Enrolamiento del tutor (RF4: obligatorio para niños < 2 años) ----------
        var edad = cliente.Edad;
        if (edad < 2 && string.IsNullOrWhiteSpace(modelo.HuellaTutorTemplateBase64))
            return (false, "Los niños menores de 2 años requieren la huella biométrica de un tutor para ingresar.", cliente.ClienteId);

        if (!string.IsNullOrWhiteSpace(modelo.HuellaTutorTemplateBase64))
        {
            try
            {
                var templateTutor = Convert.FromBase64String(modelo.HuellaTutorTemplateBase64);
                var duplicadoTutor = await _context.Tutores.AnyAsync(t =>
                    t.HuellaTemplate != null && t.HuellaTemplate.SequenceEqual(templateTutor));
                if (duplicadoTutor)
                    return (false, "Esta huella de tutor ya está registrada en el sistema.", cliente.ClienteId);

                // Se asigna al tutor principal de la familia (RF4); si no hay principal,
                // se usa el primer tutor registrado
                var tutorConHuella = familia.Tutores.FirstOrDefault(t => t.EsPrincipal)
                                     ?? familia.Tutores.FirstOrDefault();
                if (tutorConHuella == null)
                    return (false, "La familia no tiene tutores registrados para guardar la huella.", cliente.ClienteId);

                tutorConHuella.HuellaTemplate = templateTutor;
                await _context.SaveChangesAsync();
            }
            catch (FormatException)
            {
                // Template corrupto: se omite pero no bloquea el registro del niño.
            }
        }

        await _biometria.RegistrarEventoAsync("Enrolamiento",
            $"Enrolamiento completado para '{cliente.Nombre}' ({familia.ApellidosFamilia}). " +
            $"Huella niño: OK. " +
            $"Huella tutor: {(familia.Tutores.Any(t => t.HuellaTemplate != null) ? "OK" : "No aplicable")}.",
            "Info", usuarioId, cliente.ClienteId, ip);

        return (true, $"Cliente '{cliente.Nombre}' registrado y enrolado correctamente.", cliente.ClienteId);
    }

    public async Task<(bool Exito, string Mensaje)> ActualizarAsync(ClienteEditarViewModel modelo)
    {
        var cliente = await _context.Clientes
            .FirstOrDefaultAsync(c => c.ClienteId == modelo.ClienteId);

        if (cliente == null)
            return (false, "Cliente no encontrado.");

        if (!modelo.FechaNacimiento.HasValue)
            return (false, "La fecha de nacimiento es obligatoria.");

        if (modelo.FechaNacimiento.Value > DateOnly.FromDateTime(DateTime.Now))
            return (false, "La fecha de nacimiento no puede ser futura.");

        cliente.Nombre = modelo.Nombre.Trim();
        cliente.FechaNacimiento = modelo.FechaNacimiento.Value;
        cliente.NotasMedicas = string.IsNullOrWhiteSpace(modelo.NotasMedicas) ? null : modelo.NotasMedicas.Trim();
        cliente.Nacionalidad = string.IsNullOrWhiteSpace(modelo.Nacionalidad) ? null : modelo.Nacionalidad.Trim();

        await _context.SaveChangesAsync();
        return (true, "Datos actualizados correctamente.");
    }

    /// <summary>
    /// Genera el siguiente código de familia "FAM-001" secuencial tomando el máximo
    /// numérico existente y sumando 1 (el índice único de la BD evita duplicados).
    /// </summary>
    private async Task<string> GenerarCodigoFamiliaAsync()
    {
        var codigos = await _context.Familias
            .Where(f => f.CodigoFamilia != null)
            .Select(f => f.CodigoFamilia!)
            .ToListAsync();

        var maximo = 0;
        foreach (var codigo in codigos)
        {
            if (int.TryParse(codigo.StartsWith("FAM-", StringComparison.OrdinalIgnoreCase)
                    ? codigo[4..] : null, out var numero) && numero > maximo)
            {
                maximo = numero;
            }
        }

        return $"FAM-{maximo + 1:000}";
    }
}
