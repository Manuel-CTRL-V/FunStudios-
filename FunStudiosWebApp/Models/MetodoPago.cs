namespace FunStudiosWebApp.Models;

/// <summary>
/// Catálogo de métodos de pago disponibles (Efectivo, Tarjeta, Transferencia...).
/// </summary>
public class MetodoPago
{
    public int MetodoPagoId { get; set; }

    public string NombreMetodo { get; set; } = string.Empty;
}
