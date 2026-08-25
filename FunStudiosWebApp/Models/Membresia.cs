namespace FunStudiosWebApp.Models;

/// <summary>
/// Membresía contratada por un cliente. Se conserva el historial:
/// una renovación es un nuevo registro (Clientes 1 → N Membresías).
/// </summary>
public class Membresia
{
    public int MembresiaId { get; set; }

    public int ClienteId { get; set; }

    public string NombreMembresia { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public DateOnly FechaInicio { get; set; }

    public DateOnly FechaFin { get; set; }

    public decimal Precio { get; set; }

    /// <summary>Valores posibles: Activa | Vencida | Cancelada | Pendiente</summary>
    public string Estado { get; set; } = "Activa";

    public DateTime FechaCreacion { get; set; }

    // Propiedades calculadas

    /// <summary>La membresía está vigente si su estado es Activa y la fecha actual está dentro del rango.</summary>
    public bool Vigente =>
        Estado == "Activa" &&
        DateOnly.FromDateTime(DateTime.Now) >= FechaInicio &&
        DateOnly.FromDateTime(DateTime.Now) <= FechaFin;

    // Relaciones

    public Cliente? Cliente { get; set; }

    public ICollection<EntradaHistorial> Entradas { get; set; } = new List<EntradaHistorial>();
}
