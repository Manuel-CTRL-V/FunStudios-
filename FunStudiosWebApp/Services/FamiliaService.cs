using FunStudiosWebApp.Data;
using FunStudiosWebApp.Models;
using FunStudiosWebApp.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace FunStudiosWebApp.Services;

/// <summary>Implementación de la gestión de familias y sus tutores (RF2).</summary>
public class FamiliaService : IFamiliaService
{
    private readonly FunStudiosContext _context;
    private readonly IConfiguracionService _configuracion;

    public FamiliaService(FunStudiosContext context, IConfiguracionService configuracion)
    {
        _context = context;
        _configuracion = configuracion;
    }

    public async Task<List<FamiliaListadoItemViewModel>> ObtenerListadoAsync(string? busqueda, string? estado = null)
    {
        var consulta = _context.Familias
            .Include(f => f.Tutores)
            .Include(f => f.Clientes)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var texto = busqueda.Trim().ToLower();
            consulta = consulta.Where(f =>
                f.ApellidosFamilia.ToLower().Contains(texto) ||
                (f.CodigoFamilia != null && f.CodigoFamilia.ToLower().Contains(texto)) ||
                f.Tutores.Any(t => t.Nombre.ToLower().Contains(texto)));
        }

        var familias = await consulta
            .OrderBy(f => f.ApellidosFamilia)
            .ToListAsync();

        // Presencia de los niños por familia en un solo paso (evita N+1)
        var ninosDentroPorFamilia = (await _context.EntradasHistorial
                .Where(eh => eh.FechaHoraSalida == null && familias.Select(f => f.FamiliaId).Contains(eh.FamiliaId))
                .Select(eh => eh.FamiliaId)
                .ToListAsync())
            .GroupBy(id => id)
            .ToDictionary(g => g.Key, g => g.Count());

        return familias.Select(f => new FamiliaListadoItemViewModel
        {
            FamiliaId = f.FamiliaId,
            ApellidosFamilia = f.ApellidosFamilia,
            CodigoFamilia = f.CodigoFamilia,
            CantidadTutores = f.Tutores.Count,
            TutoresTexto = string.Join(", ", f.Tutores.OrderByDescending(t => t.EsPrincipal).Select(t => t.Nombre)),
            CantidadNinos = f.Clientes.Count,
            NinosDentro = ninosDentroPorFamilia.GetValueOrDefault(f.FamiliaId),
            HuellaTutorEnrolada = f.Tutores.Any(t => t.HuellaTemplate != null),
            TutorConHuellaNombre = f.Tutores
                .FirstOrDefault(t => t.HuellaTemplate != null)
                ?.Nombre
        })
        .Where(f => estado != "con_ninos_dentro" || f.NinosDentro > 0)
        .ToList();
    }

    public async Task<FamiliaDetalleViewModel?> ObtenerDetalleAsync(int familiaId)
    {
        var checksParaPremio = await _configuracion.ObtenerChecksParaPremioAsync();

        var familia = await _context.Familias
            .Include(f => f.Tutores)
            .Include(f => f.Clientes)
                .ThenInclude(c => c.FidelidadCuenta)
            .FirstOrDefaultAsync(f => f.FamiliaId == familiaId);

        if (familia == null) return null;

        var entradasAbiertas = await _context.EntradasHistorial
            .Where(eh => eh.FechaHoraSalida == null && familia.Clientes.Select(c => c.ClienteId).Contains(eh.ClienteId))
            .Select(eh => eh.ClienteId)
            .ToListAsync();

        var tutores = familia.Tutores
            .OrderByDescending(t => t.EsPrincipal)
            .ThenBy(t => t.TutorId)
            .Select(t => new TutorViewModel
            {
                TutorId = t.TutorId,
                Nombre = t.Nombre,
                Telefono = t.Telefono,
                EsPrincipal = t.EsPrincipal,
                HuellaEnrolada = t.HuellaTemplate != null
            }).ToList();

        return new FamiliaDetalleViewModel
        {
            FamiliaId = familia.FamiliaId,
            ApellidosFamilia = familia.ApellidosFamilia,
            CodigoFamilia = familia.CodigoFamilia,
            HuellaTutorEnrolada = tutores.Any(t => t.HuellaEnrolada),
            TutorConHuellaNombre = tutores.FirstOrDefault(t => t.HuellaEnrolada)?.Nombre,
            TieneTutorPrincipal = tutores.Any(t => t.EsPrincipal),
            Tutores = tutores,
            Hijos = familia.Clientes
                .OrderBy(c => c.Nombre)
                .Select(c => new HijoFamiliaItemViewModel
                {
                    ClienteId = c.ClienteId,
                    Nombre = c.Nombre,
                    Inicial = c.Inicial,
                    Edad = c.Edad,
                    HuellaEnrolada = c.HuellaNinoTemplate != null,
                    EstaDentro = entradasAbiertas.Contains(c.ClienteId),
                    ConteoChecks = c.FidelidadCuenta?.ConteoChecks ?? 0,
                    ChecksParaPremio = checksParaPremio
                }).ToList()
        };
    }

    public async Task<(bool Exito, string Mensaje)> AgregarTutorAsync(int familiaId, string nombre, string telefono)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            return (false, "El nombre del tutor es obligatorio.");

        if (string.IsNullOrWhiteSpace(telefono))
            return (false, "El teléfono del tutor es obligatorio.");

        var familia = await _context.Familias
            .Include(f => f.Tutores)
            .FirstOrDefaultAsync(f => f.FamiliaId == familiaId);

        if (familia == null)
            return (false, "Familia no encontrada.");

        // El primer tutor de la familia queda como principal
        familia.Tutores.Add(new Tutor
        {
            Nombre = nombre.Trim(),
            Telefono = telefono.Trim(),
            EsPrincipal = !familia.Tutores.Any(t => t.EsPrincipal)
        });

        await _context.SaveChangesAsync();
        return (true, $"Tutor '{nombre.Trim()}' agregado a la familia.");
    }

    public async Task<(bool Exito, string Mensaje)> ActualizarTutorAsync(int tutorId, string nombre, string telefono, bool esPrincipal)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            return (false, "El nombre del tutor es obligatorio.");

        if (string.IsNullOrWhiteSpace(telefono))
            return (false, "El teléfono del tutor es obligatorio.");

        var tutor = await _context.Tutores
            .Include(t => t.Familia)
                .ThenInclude(f => f!.Tutores)
            .FirstOrDefaultAsync(t => t.TutorId == tutorId);

        if (tutor == null)
            return (false, "Tutor no encontrado.");

        // Al marcar un nuevo principal se quita la marca al anterior,
        // pero el único principal no se puede desmarcar (invariante)
        if (esPrincipal && !tutor.EsPrincipal)
        {
            foreach (var otro in tutor.Familia!.Tutores.Where(t => t.TutorId != tutor.TutorId))
            {
                otro.EsPrincipal = false;
            }
        }
        else if (!esPrincipal && tutor.EsPrincipal)
        {
            var quedaOtroPrincipal = tutor.Familia!.Tutores.Any(t => t.TutorId != tutor.TutorId && t.EsPrincipal);
            if (!quedaOtroPrincipal && !tutor.Familia!.Tutores.Any(t => t.TutorId != tutor.TutorId))
            {
                esPrincipal = true;
            }
        }

        tutor.Nombre = nombre.Trim();
        tutor.Telefono = telefono.Trim();
        tutor.EsPrincipal = esPrincipal;

        await _context.SaveChangesAsync();
        return (true, $"Tutor '{nombre.Trim()}' actualizado.");
    }

    public async Task<(bool Exito, string Mensaje)> EliminarTutorAsync(int tutorId)
    {
        var tutor = await _context.Tutores
            .Include(t => t.Familia)
                .ThenInclude(f => f!.Tutores)
            .FirstOrDefaultAsync(t => t.TutorId == tutorId);

        if (tutor == null)
            return (false, "Tutor no encontrado.");

        var eraPrincipal = tutor.EsPrincipal;
        var otrosTutores = tutor.Familia!.Tutores
            .Where(t => t.TutorId != tutor.TutorId)
            .ToList();

        _context.Tutores.Remove(tutor);

        // Conservar la invariante de un principal: si se eliminó al principal,
        // el primer tutor restante toma su lugar
        if (eraPrincipal)
        {
            var nuevoPrincipal = otrosTutores.FirstOrDefault();
            if (nuevoPrincipal != null) nuevoPrincipal.EsPrincipal = true;
        }

        await _context.SaveChangesAsync();
        return (true, "Tutor eliminado.");
    }
}