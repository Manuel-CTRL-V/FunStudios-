using FunStudiosWebApp.Data;
using FunStudiosWebApp.Models;
using FunStudiosWebApp.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace FunStudiosWebApp.Services;

/// <summary>Implementación de la configuración del sistema (panel de Administrador).</summary>
public class ConfiguracionService : IConfiguracionService
{
    /// <summary>Clave del parámetro que define la meta de checks de fidelidad.</summary>
    public const string ClaveChecksParaPremio = "Fidelidad.ChecksParaPremio";

    /// <summary>Clave del parámetro que define el límite diario de checks de fidelidad.</summary>
    public const string ClaveChecksPorDia = "Fidelidad.ChecksPorDia";

    private readonly FunStudiosContext _context;

    public ConfiguracionService(FunStudiosContext context)
    {
        _context = context;
    }

    public async Task<ConfiguracionIndexViewModel> ObtenerResumenAsync()
    {
        var tipos = await _context.TiposAcceso
            .OrderBy(ta => ta.TipoAccesoId)
            .ToListAsync();

        var metodos = await _context.MetodosPago
            .OrderBy(mp => mp.MetodoPagoId)
            .ToListAsync();

        var parametros = await _context.Parametros
            .OrderBy(p => p.Clave)
            .ToListAsync();

        var tiposUsados = await _context.EntradasHistorial
            .GroupBy(eh => eh.TipoAccesoId)
            .Select(g => g.Key)
            .ToListAsync();

        var metodosUsados = await _context.Ventas
            .Select(v => v.MetodoPagoId)
            .Distinct()
            .ToListAsync();

        return new ConfiguracionIndexViewModel
        {
            TiposAcceso = tipos.Select(ta => new TipoAccesoConfigViewModel
            {
                TipoAccesoId = ta.TipoAccesoId,
                NombreAcceso = ta.NombreAcceso,
                Precio = ta.Precio,
                Descripcion = ta.Descripcion,
                Activo = ta.Activo,
                UsadoEnEntradas = tiposUsados.Contains(ta.TipoAccesoId)
            }).ToList(),
            MetodosPago = metodos.Select(mp => new MetodoPagoConfigViewModel
            {
                MetodoPagoId = mp.MetodoPagoId,
                NombreMetodo = mp.NombreMetodo,
                EnUso = metodosUsados.Contains(mp.MetodoPagoId)
            }).ToList(),
            Parametros = parametros.Select(p => new ParametroConfigViewModel
            {
                ParametroId = p.ParametroId,
                Clave = p.Clave,
                Valor = p.Valor,
                Descripcion = p.Descripcion,
                Tipo = p.Tipo,
                NombreVisible = p.NombreVisible
            }).ToList()
        };
    }

    public async Task<int> ObtenerChecksParaPremioAsync()
    {
        var valor = await _context.Parametros
            .Where(p => p.Clave == ClaveChecksParaPremio)
            .Select(p => p.Valor)
            .FirstOrDefaultAsync();

        return int.TryParse(valor, out var checks) && checks > 0 ? checks : 8;
    }

    public async Task<int> ObtenerChecksPorDiaAsync()
    {
        var valor = await _context.Parametros
            .Where(p => p.Clave == ClaveChecksPorDia)
            .Select(p => p.Valor)
            .FirstOrDefaultAsync();

        return int.TryParse(valor, out var checks) && checks > 0 ? checks : 1;
    }

    // ---------- Tipos de acceso ----------

    public async Task<TipoAccesoEditarViewModel?> ObtenerTipoAccesoParaEditarAsync(int id)
    {
        var ta = await _context.TiposAcceso.FindAsync(id);
        if (ta == null) return null;

        return new TipoAccesoEditarViewModel
        {
            TipoAccesoId = ta.TipoAccesoId,
            NombreAcceso = ta.NombreAcceso,
            Precio = ta.Precio,
            Descripcion = ta.Descripcion,
            Activo = ta.Activo
        };
    }

    public async Task<(bool Exito, string Mensaje)> GuardarTipoAccesoAsync(TipoAccesoEditarViewModel modelo)
    {
        if (string.IsNullOrWhiteSpace(modelo.NombreAcceso))
            return (false, "El nombre del tipo de acceso es obligatorio.");

        if (modelo.Precio < 0)
            return (false, "El precio no puede ser negativo.");

        var ta = await _context.TiposAcceso.FindAsync(modelo.TipoAccesoId);
        if (ta == null)
            return (false, "Tipo de acceso no encontrado.");

        var nombre = modelo.NombreAcceso.Trim();
        var duplicado = await _context.TiposAcceso.AnyAsync(x =>
            x.TipoAccesoId != ta.TipoAccesoId && x.NombreAcceso.ToLower() == nombre.ToLower());

        if (duplicado)
            return (false, "Ya existe un tipo de acceso con ese nombre.");

        ta.NombreAcceso = nombre;
        ta.Precio = modelo.Precio;
        ta.Descripcion = string.IsNullOrWhiteSpace(modelo.Descripcion) ? null : modelo.Descripcion.Trim();
        ta.Activo = modelo.Activo;

        await _context.SaveChangesAsync();
        return (true, $"Tipo de acceso '{nombre}' actualizado.");
    }

    // ---------- Métodos de pago ----------

    public async Task<MetodoPagoFormViewModel?> ObtenerMetodoPagoParaEditarAsync(int id)
    {
        var mp = await _context.MetodosPago.FindAsync(id);
        if (mp == null) return null;

        return new MetodoPagoFormViewModel
        {
            MetodoPagoId = mp.MetodoPagoId,
            NombreMetodo = mp.NombreMetodo
        };
    }

    public async Task<(bool Exito, string Mensaje)> CrearMetodoPagoAsync(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            return (false, "El nombre del método de pago es obligatorio.");

        var nombreLimpio = nombre.Trim();
        var duplicado = await _context.MetodosPago.AnyAsync(mp => mp.NombreMetodo.ToLower() == nombreLimpio.ToLower());
        if (duplicado)
            return (false, "Ya existe un método de pago con ese nombre.");

        _context.MetodosPago.Add(new MetodoPago { NombreMetodo = nombreLimpio });
        await _context.SaveChangesAsync();
        return (true, $"Método de pago '{nombreLimpio}' creado.");
    }

    public async Task<(bool Exito, string Mensaje)> ActualizarMetodoPagoAsync(int id, string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            return (false, "El nombre del método de pago es obligatorio.");

        var mp = await _context.MetodosPago.FindAsync(id);
        if (mp == null)
            return (false, "Método de pago no encontrado.");

        var nombreLimpio = nombre.Trim();
        var duplicado = await _context.MetodosPago.AnyAsync(x =>
            x.MetodoPagoId != mp.MetodoPagoId && x.NombreMetodo.ToLower() == nombreLimpio.ToLower());

        if (duplicado)
            return (false, "Ya existe un método de pago con ese nombre.");

        mp.NombreMetodo = nombreLimpio;
        await _context.SaveChangesAsync();
        return (true, $"Método de pago '{nombreLimpio}' actualizado.");
    }

    public async Task<(bool Exito, string Mensaje)> EliminarMetodoPagoAsync(int id)
    {
        var mp = await _context.MetodosPago.FindAsync(id);
        if (mp == null)
            return (false, "Método de pago no encontrado.");

        var enUso = await _context.Ventas.AnyAsync(v => v.MetodoPagoId == id);
        if (enUso)
            return (false, $"No se puede eliminar '{mp.NombreMetodo}' porque ya tiene ventas asociadas.");

        _context.MetodosPago.Remove(mp);
        await _context.SaveChangesAsync();
        return (true, $"Método de pago '{mp.NombreMetodo}' eliminado.");
    }

    // ---------- Parámetros ----------

    public async Task<ParametroEditarViewModel?> ObtenerParametroParaEditarAsync(int id)
    {
        var p = await _context.Parametros.FindAsync(id);
        if (p == null) return null;

        return new ParametroEditarViewModel
        {
            ParametroId = p.ParametroId,
            Clave = p.Clave,
            Valor = p.Valor,
            Descripcion = p.Descripcion,
            Tipo = p.Tipo,
            NombreVisible = p.NombreVisible
        };
    }

    public async Task<(bool Exito, string Mensaje)> GuardarParametroAsync(ParametroEditarViewModel modelo)
    {
        if (string.IsNullOrWhiteSpace(modelo.Valor))
            return (false, "El valor del parámetro es obligatorio.");

        var valor = modelo.Valor.Trim();
        switch (modelo.Tipo)
        {
            case "numero" when !int.TryParse(valor, out _):
                return (false, "El valor debe ser un número entero.");
            case "decimal" when !decimal.TryParse(valor, out _):
                return (false, "El valor debe ser un número decimal.");
            case "bool" when !bool.TryParse(valor, out _):
                return (false, "El valor debe ser true o false.");
        }

        var parametro = await _context.Parametros.FindAsync(modelo.ParametroId);
        if (parametro == null)
            return (false, "Parámetro no encontrado.");

        parametro.Valor = valor;
        await _context.SaveChangesAsync();
        return (true, $"Parámetro '{parametro.Clave}' actualizado a '{valor}'.");
    }
}