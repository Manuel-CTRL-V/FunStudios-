using FunStudiosWebApp.Models;
using FunStudiosWebApp.ViewModels;

namespace FunStudiosWebApp.Services;

/// <summary>
/// Lógica del checkpoint de acceso: identificación manual del cliente, registro de
/// entradas y salidas, acumulación de checks de fidelidad (RF5 a RF10).
/// </summary>
public interface IAccesoService
{
    /// <summary>Arma el resultado del checkpoint para un cliente ya identificado.</summary>
    Task<ResultadoEscaneoViewModel> ObtenerResultadoClienteAsync(Cliente cliente);

    /// <summary>Registra la hora de entrada del cliente (RF6) aplicando las reglas de fidelidad.</summary>
    Task<ResultadoRegistroEntradaViewModel> RegistrarEntradaAsync(RegistrarEntradaRequest solicitud, int? usuarioId, string ip);

    /// <summary>Registra la hora de salida del cliente (RF7).</summary>
    Task<ResultadoRegistroEntradaViewModel> RegistrarSalidaAsync(int clienteId, string ip, int? usuarioId = null);
}
