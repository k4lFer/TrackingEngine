using App.Objects.Gps.DTOs;

namespace App.Interfaces.Ports.Tracking;

/// <summary>
/// Decodificador de protocolo GPS genérico. Cada implementación entiende un
/// protocolo (OsmAnd, TK103, Teltonika, ...) y traduce sus tramas a lecturas
/// normalizadas <see cref="DeviceGpsReading"/>.
/// </summary>
public interface IGpsDeviceDecoder
{
    /// <summary>Nombre legible del protocolo (para logs).</summary>
    string Name { get; }

    /// <summary>true si el payload probablemente pertenece a este protocolo.</summary>
    bool CanDecode(ReadOnlySpan<byte> payload);

    /// <summary>
    /// Intenta decodificar el payload completo de una trama.
    /// Devuelve false si no se pudo; true (posiblemente con lista vacía) si era
    /// una trama del protocolo sin posición útil.
    /// </summary>
    bool TryDecode(ReadOnlySpan<byte> payload, out IReadOnlyList<DeviceGpsReading> readings);
}