using FunStudiosWebApp.Data;
using FunStudiosWebApp.Models;
using FunStudiosWebApp.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace FunStudiosWebApp.Services;

/// <summary>Implementación de la gestión de clientes.</summary>
public class ClienteService : IClienteService
{
    private readonly FunStudiosContext _context;
    private readonly IConfiguracionService _configuracion;

    public ClienteService(FunStudiosContext context, IConfiguracionService configuracion)
    {
        _context = context;
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
                TieneFechaNacimiento = !c.SinFechaNacimiento,
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
            TieneFechaNacimiento = !cliente.SinFechaNacimiento,
            NotasMedicas = cliente.NotasMedicas,
            Nacionalidad = cliente.Nacionalidad,
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
                    EsPrincipal = t.EsPrincipal
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

    public async Task<(bool Exito, string Mensaje, int ClienteId)> RegistrarAsync(ClienteCrearViewModel modelo)
    {
        if (modelo.FamiliaExistenteId == null)
            return (false, "Seleccione la familia del cliente.", 0);

        var familiaExiste = await _context.Familias
            .AnyAsync(f => f.FamiliaId == modelo.FamiliaExistenteId.Value);

        if (!familiaExiste)
            return (false, "La familia seleccionada no existe.", 0);

        if (modelo.FechaNacimiento.HasValue &&
            modelo.FechaNacimiento.Value > DateOnly.FromDateTime(DateTime.Now))
        {
            return (false, "La fecha de nacimiento no puede ser futura.", 0);
        }

        var cliente = new Cliente
        {
            FamiliaId = modelo.FamiliaExistenteId.Value,
            Nombre = string.IsNullOrWhiteSpace(modelo.Nombre) ? string.Empty : modelo.Nombre.Trim(),
            FechaNacimiento = modelo.FechaNacimiento ?? Cliente.FechaNacimientoSinRegistrar,
            NotasMedicas = string.IsNullOrWhiteSpace(modelo.NotasMedicas) ? null : modelo.NotasMedicas.Trim(),
            Nacionalidad = string.IsNullOrWhiteSpace(modelo.Nacionalidad) ? null : modelo.Nacionalidad.Trim()
        };

        _context.Clientes.Add(cliente);

        // Cuenta de fidelidad inicial (relación 1 a 1)
        cliente.FidelidadCuenta = new FidelidadCuenta
        {
            ConteoChecks = 0,
            EntradasGratisGanadas = 0
        };

        await _context.SaveChangesAsync();

        return (true, $"{cliente.NombreMostrar} registrado correctamente.", cliente.ClienteId);
    }

    public async Task<(bool Exito, string Mensaje)> ActualizarAsync(ClienteEditarViewModel modelo)
    {
        var cliente = await _context.Clientes
            .FirstOrDefaultAsync(c => c.ClienteId == modelo.ClienteId);

        if (cliente == null)
            return (false, "Cliente no encontrado.");

        if (modelo.FechaNacimiento.HasValue &&
            modelo.FechaNacimiento.Value > DateOnly.FromDateTime(DateTime.Now))
        {
            return (false, "La fecha de nacimiento no puede ser futura.");
        }

        cliente.Nombre = string.IsNullOrWhiteSpace(modelo.Nombre) ? string.Empty : modelo.Nombre.Trim();
        cliente.FechaNacimiento = modelo.FechaNacimiento ?? Cliente.FechaNacimientoSinRegistrar;
        cliente.NotasMedicas = string.IsNullOrWhiteSpace(modelo.NotasMedicas) ? null : modelo.NotasMedicas.Trim();
        cliente.Nacionalidad = string.IsNullOrWhiteSpace(modelo.Nacionalidad) ? null : modelo.Nacionalidad.Trim();

        await _context.SaveChangesAsync();
        return (true, "Datos actualizados correctamente.");
    }

    public async Task<List<ClienteBusquedaViewModel>> BuscarParaCheckpointAsync(string? texto, int limite = 20)
    {
        limite = Math.Clamp(limite, 1, 50);

        var consulta = _context.Clientes
            .Include(c => c.Familia)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(texto))
        {
            var q = texto.Trim().ToLower();
            consulta = consulta.Where(c =>
                c.Nombre.ToLower().Contains(q) ||
                c.Familia!.ApellidosFamilia.ToLower().Contains(q) ||
                (c.Familia!.CodigoFamilia != null && c.Familia!.CodigoFamilia.ToLower().Contains(q)));
        }

        var clientes = await consulta
            .OrderBy(c => c.Nombre)
            .ThenBy(c => c.ClienteId)
            .Take(limite)
            .ToListAsync();

        var ids = clientes.Select(c => c.ClienteId).ToList();

        var dentro = (await _context.EntradasHistorial
                .Where(eh => eh.FechaHoraSalida == null && ids.Contains(eh.ClienteId))
                .Select(eh => eh.ClienteId)
                .Distinct()
                .ToListAsync())
            .ToHashSet();

        return clientes.Select(c => new ClienteBusquedaViewModel
        {
            ClienteId = c.ClienteId,
            Nombre = c.Nombre,
            Inicial = c.Inicial,
            ApellidosFamilia = c.Familia?.ApellidosFamilia ?? string.Empty,
            CodigoFamilia = c.Familia?.CodigoFamilia,
            EdadTexto = c.EdadTexto,
            EstaDentro = dentro.Contains(c.ClienteId)
        }).ToList();
    }
}
