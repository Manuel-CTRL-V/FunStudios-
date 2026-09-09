namespace FunStudiosWebApp.Models;

/// <summary>
/// Parámetro clave-valor de configuración general del sistema (tabla Parametros).
/// Permite parametrizar valores de negocio sin cambios de código (checks de
/// fidelidad, símbolo de moneda, nombre del parque, etc.).
/// </summary>
public class Parametro
{
    public int ParametroId { get; set; }

    /// <summary>Clave única (ej. "Fidelidad.ChecksParaPremio").</summary>
    public string Clave { get; set; } = string.Empty;

    /// <summary>Valor guardado (texto, número o booleano según Tipo).</summary>
    public string? Valor { get; set; }

    /// <summary>Texto explicativo de qué controla el parámetro.</summary>
    public string? Descripcion { get; set; }

    /// <summary>"texto" | "numero" | "decimal" | "bool".</summary>
    public string Tipo { get; set; } = "texto";

    /// <summary>Etiqueta amigable para mostrarla en el panel de Configuración.</summary>
    public string? NombreVisible { get; set; }
}