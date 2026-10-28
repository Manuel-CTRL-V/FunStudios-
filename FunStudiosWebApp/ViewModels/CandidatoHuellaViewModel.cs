namespace FunStudiosWebApp.ViewModels;

public class CandidatoHuellaViewModel
{
    public string TemplateBase64 { get; set; } = string.Empty;
    public int ClienteId { get; set; }
}

public class RespuestaCandidatosHuellaViewModel
{
    public bool Exitoso { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public List<CandidatoHuellaViewModel> Candidatos { get; set; } = new();
}

public class IdentificarResultadoRequest
{
    public int Indice { get; set; }
    public int? ClienteIdResuelto { get; set; }
}
