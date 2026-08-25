namespace FunStudiosWebApp.Models;

/// <summary>
/// Registra las ventas realizadas (ej. cobro de un Day Pass). MetodoPago 1 → N Venta.
/// </summary>
public class Venta
{
    public int VentaId { get; set; }

    public int ClienteId { get; set; }

    public int MetodoPagoId { get; set; }

    public decimal Monto { get; set; }

    public string Descripcion { get; set; } = string.Empty;

    public DateTime FechaVenta { get; set; }

    // Relaciones

    public Cliente? Cliente { get; set; }

    public MetodoPago? MetodoPago { get; set; }
}
