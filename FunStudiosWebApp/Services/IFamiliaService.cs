using FunStudiosWebApp.ViewModels;

namespace FunStudiosWebApp.Services;

/// <summary>
/// Gestión de familias: listado, perfil y administración de los tutores
/// de cada grupo familiar (RF2).
/// </summary>
public interface IFamiliaService
{
    Task<List<FamiliaListadoItemViewModel>> ObtenerListadoAsync(string? busqueda, string? estado = null);

    Task<FamiliaDetalleViewModel?> ObtenerDetalleAsync(int familiaId);

    Task<(bool Exito, string Mensaje)> AgregarTutorAsync(int familiaId, string nombre, string telefono);

    Task<(bool Exito, string Mensaje)> ActualizarTutorAsync(int tutorId, string nombre, string telefono, bool esPrincipal);

    Task<(bool Exito, string Mensaje)> EliminarTutorAsync(int tutorId);
}