using FunStudiosWebApp.ViewModels;

namespace FunStudiosWebApp.Services;

/// <summary>Gestión de clientes: listado, perfil, registro con enrolamiento biométrico y edición.</summary>
public interface IClienteService
{
    Task<List<ClienteListadoItemViewModel>> ObtenerListadoAsync(string? busqueda, string? estado);

    Task<ClienteDetalleViewModel?> ObtenerDetalleAsync(int clienteId);

    /// <summary>Familias disponibles para vincular un nuevo niño (RF2).</summary>
    Task<List<FamiliaOpcionViewModel>> ObtenerFamiliasAsync();

    /// <summary>Registra un niño dentro de una familia existente (RF1, RF2).</summary>
    Task<(bool Exito, string Mensaje, int ClienteId)> RegistrarAsync(ClienteCrearViewModel modelo);

    /// <summary>Actualiza datos básicos del niño y teléfonos de la familia.</summary>
    Task<(bool Exito, string Mensaje)> ActualizarAsync(ClienteEditarViewModel modelo);

    /// <summary>Búsqueda manual de clientes para el checkpoint (nombre, familia o código).</summary>
    Task<List<ClienteBusquedaViewModel>> BuscarParaCheckpointAsync(string? texto, int limite = 20);
}
