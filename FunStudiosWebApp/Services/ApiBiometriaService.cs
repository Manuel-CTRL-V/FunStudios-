using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FunStudiosWebApp.Data;
using FunStudiosWebApp.Models;

namespace FunStudiosWebApp.Services;

/// <summary>
/// Implementación real de <see cref="IBiometriaService"/> que consume la API local
/// <c>BiometricService</c> (que corre en la PC de recepción con el lector DigitalPersona)
/// por HTTP. El punto de red lo define la configuración <c>BiometriaApi:BaseUrl</c>.
/// </summary>
public class ApiBiometriaService : IBiometriaService
{
    private readonly HttpClient _http;
    private readonly FunStudiosContext _context;

    public ApiBiometriaService(HttpClient http, FunStudiosContext context)
    {
        _http = http;
        _context = context;
    }

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<EstadoBiometria> EstadoAsync(CancellationToken ct = default)
    {
        var resultado = new EstadoBiometria { LectorConectado = false };
        try
        {
            var response = await _http.GetAsync("api/biometria/estado", ct);
            if (response.IsSuccessStatusCode)
            {
                var dto = await response.Content.ReadFromJsonAsync<EstadoResponseDto>(JsonOpts, ct);
                resultado.LectorConectado = dto?.LectorConectado ?? false;
                resultado.NombreDispositivo = dto?.NombreDispositivo ?? "";
                resultado.Ocupado = dto?.Ocupado ?? false;
            }
        }
        catch
        {
            // Lector no disponible en este momento.
        }
        return resultado;
    }

    public async Task<string?> EnrolarAsync(int muestras, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("api/biometria/enrolar",
            new { muestras }, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<EnrolarResponseDto>(JsonOpts, ct);
        return result is { Exitoso: true } ? result.TemplateBase64 : null;
    }

    public async Task<int> IdentificarAsync(IReadOnlyList<string> templatesBase64, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("api/biometria/identificar",
            new { templatesBase64 }, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<IdentificarResponseDto>(JsonOpts, ct);
        return result is { Exitoso: true, Coincide: true } ? result.IndiceCoincidencia : -1;
    }

    public async Task<bool> VerificarAsync(string templateBase64, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("api/biometria/verificar",
            new { templateBase64 }, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<VerificarResponseDto>(JsonOpts, ct);
        return result is { Exitoso: true, Coincide: true };
    }

    public async Task RegistrarEventoAsync(string operacion, string mensaje, string gravedad,
        int? usuarioId = null, int? clienteId = null, string ip = "")
    {
        _context.LogsBiometrico.Add(new LogBiometrico
        {
            FechaHoraEvento = DateTime.Now,
            Gravedad = gravedad,
            Operacion = operacion,
            MensajeDetalle = mensaje,
            IPEstacionTrabajo = string.IsNullOrWhiteSpace(ip) ? "127.0.0.1" : ip,
            UsuarioId = usuarioId,
            ClienteId = clienteId
        });

        await _context.SaveChangesAsync();
    }

    #region DTOs de la API remota

    private sealed class EstadoResponseDto
    {
        [JsonPropertyName("lectorConectado")] public bool LectorConectado { get; set; }
        [JsonPropertyName("nombreDispositivo")] public string NombreDispositivo { get; set; } = "";
        [JsonPropertyName("ocupado")] public bool Ocupado { get; set; }
    }

    private sealed class EnrolarResponseDto
    {
        [JsonPropertyName("exitoso")] public bool Exitoso { get; set; }
        [JsonPropertyName("templateBase64")] public string? TemplateBase64 { get; set; }
    }

    private sealed class IdentificarResponseDto
    {
        [JsonPropertyName("exitoso")] public bool Exitoso { get; set; }
        [JsonPropertyName("coincide")] public bool Coincide { get; set; }
        [JsonPropertyName("indiceCoincidencia")] public int IndiceCoincidencia { get; set; }
    }

    private sealed class VerificarResponseDto
    {
        [JsonPropertyName("exitoso")] public bool Exitoso { get; set; }
        [JsonPropertyName("coincide")] public bool Coincide { get; set; }
    }

    #endregion
}