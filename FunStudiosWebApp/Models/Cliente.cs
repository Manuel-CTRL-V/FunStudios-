namespace FunStudiosWebApp.Models;

/// <summary>
/// Niño/cliente que ingresa al parque. El acceso se identifica por huella dactilar (RF1, RF3).
/// </summary>
public class Cliente
{
    public int ClienteId { get; set; }

    public int FamiliaId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public DateOnly FechaNacimiento { get; set; }

    public string? NotasMedicas { get; set; }

    /// <summary>
    /// Template biométrico del niño (RF3). En producción se guarda encriptado (RNF4).
    /// Null mientras no se complete el enrolamiento de 3 muestras.
    /// </summary>
    public byte[]? HuellaNinoTemplate { get; set; }

    // Propiedades calculadas

    /// <summary>Edad exacta del niño a la fecha actual.</summary>
    public int Edad
    {
        get
        {
            var hoy = DateOnly.FromDateTime(DateTime.Now);
            var edad = hoy.Year - FechaNacimiento.Year;
            if (FechaNacimiento.AddYears(edad) > hoy) edad--;
            return edad;
        }
    }

    /// <summary>Inicial para el avatar circular de la interfaz.</summary>
    public string Inicial => string.IsNullOrEmpty(Nombre) ? "?" : Nombre[..1].ToUpper();

    /// <summary>Indica si el niño tiene menos de 2 años (aplica enrolamiento opcional del tutor, RF4).</summary>
    public bool MenorDeDosAnios => Edad < 2;

    // Relaciones

    public Familia? Familia { get; set; }

    public ICollection<EntradaHistorial> Entradas { get; set; } = new List<EntradaHistorial>();

    /// <summary>Cuenta de fidelidad 1 a 1.</summary>
    public FidelidadCuenta? FidelidadCuenta { get; set; }

    public ICollection<Membresia> Membresias { get; set; } = new List<Membresia>();

    public ICollection<Venta> Ventas { get; set; } = new List<Venta>();

    public ICollection<LogBiometrico> LogsBiometrico { get; set; } = new List<LogBiometrico>();
}
