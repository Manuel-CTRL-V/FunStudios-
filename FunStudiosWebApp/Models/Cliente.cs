namespace FunStudiosWebApp.Models;

/// <summary>
/// Niño/cliente que ingresa al parque. El acceso se registra manualmente en el checkpoint (RF1, RF5).
/// </summary>
public class Cliente
{
    /// <summary>
    /// Fecha centinela para clientes sin fecha de nacimiento registrada.
    /// La columna en base de datos es NOT NULL, así que se guarda este valor
    /// y la interfaz lo interpreta como "Edad no registrada".
    /// </summary>
    public static readonly DateOnly FechaNacimientoSinRegistrar = new(1900, 1, 1);

    public int ClienteId { get; set; }

    public int FamiliaId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public DateOnly FechaNacimiento { get; set; }

    /// <summary>true cuando el cliente no tiene fecha de nacimiento registrada.</summary>
    public bool SinFechaNacimiento => FechaNacimiento == FechaNacimientoSinRegistrar;

    public string? NotasMedicas { get; set; }

    /// <summary>Nacionalidad del niño (opcional, default "Dominicana" en la UI).</summary>
    public string? Nacionalidad { get; set; }

    /// <summary>
    /// Template biométrico del niño (RF3). En producción se guarda encriptado (RNF4).
    /// Null mientras no se complete el enrolamiento de 3 muestras.
    /// </summary>
    public byte[]? HuellaNinoTemplate { get; set; }

    // Propiedades calculadas

    /// <summary>Edad exacta del niño a la fecha actual (0 si no está registrada).</summary>
    public int Edad
    {
        get
        {
            if (SinFechaNacimiento) return 0;

            var hoy = DateOnly.FromDateTime(DateTime.Now);
            var edad = hoy.Year - FechaNacimiento.Year;
            if (FechaNacimiento.AddYears(edad) > hoy) edad--;
            return edad;
        }
    }

    /// <summary>Edad legible para la interfaz; "Edad no registrada" con la fecha centinela.</summary>
    public string EdadTexto => SinFechaNacimiento ? "Edad no registrada" : $"{Edad} años";

    /// <summary>Nombre para mostrar; usa "Cliente #id" cuando no se registró nombre.</summary>
    public string NombreMostrar => string.IsNullOrWhiteSpace(Nombre) ? $"Cliente #{ClienteId}" : Nombre;

    /// <summary>Inicial para el avatar circular de la interfaz.</summary>
    public string Inicial => string.IsNullOrEmpty(Nombre) ? "?" : Nombre[..1].ToUpper();

    /// <summary>Indica si el niño tiene menos de 2 años (dato histórico, RF4).</summary>
    public bool MenorDeDosAnios => !SinFechaNacimiento && Edad < 2;

    // Relaciones

    public Familia? Familia { get; set; }

    public ICollection<EntradaHistorial> Entradas { get; set; } = new List<EntradaHistorial>();

    /// <summary>Cuenta de fidelidad 1 a 1.</summary>
    public FidelidadCuenta? FidelidadCuenta { get; set; }

    public ICollection<Membresia> Membresias { get; set; } = new List<Membresia>();

    public ICollection<Venta> Ventas { get; set; } = new List<Venta>();

    public ICollection<LogBiometrico> LogsBiometrico { get; set; } = new List<LogBiometrico>();
}
