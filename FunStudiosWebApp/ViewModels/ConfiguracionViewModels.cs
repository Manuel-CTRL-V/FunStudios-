namespace FunStudiosWebApp.ViewModels;

/// <summary>Modelo completo de la pantalla Configuración (engranaje del header).</summary>
public class ConfiguracionIndexViewModel
{
    public List<TipoAccesoConfigViewModel> TiposAcceso { get; set; } = new();
    public List<MetodoPagoConfigViewModel> MetodosPago { get; set; } = new();
    public List<ParametroConfigViewModel> Parametros { get; set; } = new();
}

/// <summary>Fila del catálogo de tipos de acceso.</summary>
public class TipoAccesoConfigViewModel
{
    public int TipoAccesoId { get; set; }
    public string NombreAcceso { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public string? Descripcion { get; set; }
    public bool Activo { get; set; }

    /// <summary>Indica si el tipo ya fue usado en entradas (protección al desactivar).</summary>
    public bool UsadoEnEntradas { get; set; }
}

/// <summary>Formulario de edición de un tipo de acceso.</summary>
public class TipoAccesoEditarViewModel
{
    public int TipoAccesoId { get; set; }
    public string NombreAcceso { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public string? Descripcion { get; set; }
    public bool Activo { get; set; }
}

/// <summary>Fila del catálogo de métodos de pago.</summary>
public class MetodoPagoConfigViewModel
{
    public int MetodoPagoId { get; set; }
    public string NombreMetodo { get; set; } = string.Empty;

    /// <summary>Indica si ya se usó en ventas (bloquea borrar).</summary>
    public bool EnUso { get; set; }
}

/// <summary>Formulario de alta/edición de un método de pago.</summary>
public class MetodoPagoFormViewModel
{
    public int MetodoPagoId { get; set; }
    public string NombreMetodo { get; set; } = string.Empty;
}

/// <summary>Fila de la tabla de parámetros clave-valor.</summary>
public class ParametroConfigViewModel
{
    public int ParametroId { get; set; }
    public string Clave { get; set; } = string.Empty;
    public string? Valor { get; set; }
    public string? Descripcion { get; set; }
    public string Tipo { get; set; } = "texto";
    public string? NombreVisible { get; set; }
}

/// <summary>Formulario de edición del valor de un parámetro.</summary>
public class ParametroEditarViewModel
{
    public int ParametroId { get; set; }
    public string Clave { get; set; } = string.Empty;
    public string? Valor { get; set; }
    public string? Descripcion { get; set; }
    public string Tipo { get; set; } = "texto";
    public string? NombreVisible { get; set; }
}