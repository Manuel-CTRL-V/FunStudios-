using System.Security.Cryptography;
using FunStudiosWebApp.Data;

namespace FunStudiosWebApp.Services;

/// <summary>
/// Implementación SIMULADA de la interfaz biométrica mientras no existe el
/// dispositivo DigitalPersona ni el servicio local de Windows (RNF3).
/// Genera templates aleatorios que imitan el comportamiento del SDK real:
/// cada captura produce bytes distintos y la verificación solo es positiva
/// cuando se compara un template consigo mismo (identificación 1:N realista).
/// </summary>
public class SimuladorBiometriaService : IBiometriaService
{
    private readonly FunStudiosContext _context;

    public SimuladorBiometriaService(FunStudiosContext context)
    {
        _context = context;
    }

    public string NombreDispositivo => "DigitalPersona U.are.U (SIMULADO)";

    public bool LectorConectado => true;

    public Task<byte[]> CapturarMuestraAsync()
    {
        // Simula el tiempo que tarda el lector en capturar una muestra.
        return Task.FromResult(RandomNumberGenerator.GetBytes(64));
    }

    public Task<bool> VerificarAsync(byte[] muestraActual, byte[] templateRegistrado)
    {
        if (muestraActual.Length == 0 || templateRegistrado.Length == 0 ||
            muestraActual.Length != templateRegistrado.Length)
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(muestraActual.AsSpan().SequenceEqual(templateRegistrado));
    }

    public async Task RegistrarEventoAsync(string operacion, string mensaje, string gravedad,
        int? usuarioId = null, int? clienteId = null, string ip = "")
    {
        _context.LogsBiometrico.Add(new Models.LogBiometrico
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
}
