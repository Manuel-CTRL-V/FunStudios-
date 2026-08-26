using FunStudiosWebApp.ViewModels;

namespace FunStudiosWebApp.Services;

/// <summary>Gestión de clientes: listado, perfil, registro con enrolamiento biométrico y edición.</summary>
public interface IClienteService
{
    Task<List<ClienteListadoItemViewModel>> ObtenerListadoAsync(string? busqueda, string? estado);

    Task<ClienteDetalleViewModel?> ObtenerDetalleAsync(int clienteId);

    /// <summary>Familias disponibles para vincular un nuevo niño (RF2).</summary>
    Task<List<FamiliaOpcionViewModel>> ObtenerFamiliasAsync();

    /// <summary>Registra familia + niño + enrolamiento biométrico (RF1 a RF4).</summary>
    Task<(bool Exito, string Mensaje, int ClienteId)> RegistrarAsync(ClienteCrearViewModel modelo,
        int? usuarioId, string ip);

    /// <summary>Actualiza datos básicos del niño y teléfonos de la familia.</summary>
    Task<(bool Exito, string Mensaje)> ActualizarAsync(ClienteEditarViewModel modelo);
}
