using System.Collections.Concurrent;
using App.Interfaces.Ports.Tracking;
using App.Shared.Common.Enums;
using App.Shared.Common.Gps;
using Microsoft.Extensions.Options;

namespace App.UseCases.Gps;

/// <summary>
/// Filtro de tramas repetidas, en memoria y singleton.
///
/// Descarta la trama si el vehículo lleva poco tiempo detenido (Rama 1) o si está
/// casi en el mismo sitio que el último punto guardado (Rama 2). En ambos casos deja
/// pasar un punto cada <see cref="GpsRealtimeOptions.MaxSuppressSeconds"/> como
/// máximo, para que la traza no se corte.
///
/// En ambos casos deja pasar SIEMPRE un cambio de estado. Un filtro ingenuo por
/// distancia retendría la trama de la frenada hasta 120 s, y con ella la transición
/// a "Detenido" y el arranque de la duración de parada.
/// </summary>
public sealed class VehicleRealtimeGate : IVehicleRealtimeGate
{
    private readonly ConcurrentDictionary<Guid, GateEntry> _entries = new();
    private readonly ConcurrentDictionary<Guid, DateTime> _lastHeartbeatAt = new();
    private readonly decimal _stopSpeedKmh;
    private readonly double _minDistanceMeters;
    private readonly TimeSpan _maxSuppress;
    private readonly TimeSpan _heartbeatWindow;
    private readonly int _maxTrackedVehicles;
    private long _purges;
    private long _lastBroadcastUtcTicks;

    public VehicleRealtimeGate(IOptions<GpsRealtimeOptions> options)
    {
        var o = options.Value;
        _stopSpeedKmh = o.StopSpeedKmh;
        _minDistanceMeters = o.MinDistanceMeters;
        _maxSuppress = TimeSpan.FromSeconds(Math.Max(1, o.MaxSuppressSeconds));
        _heartbeatWindow = TimeSpan.FromSeconds(Math.Max(1, o.HeartbeatSeconds));
        _maxTrackedVehicles = Math.Max(1, o.MaxTrackedVehicles);
    }

    public GpsGateDecision Evaluate(Guid vehicleId, double latitude, double longitude, decimal? speedKmh, DateTime nowUtc)
    {
        PurgeIfNeeded();

        var producedState = (speedKmh ?? 0) < _stopSpeedKmh
            ? VehicleState.Detenido
            : VehicleState.EnRuta;

        var decision = GpsGateDecision.Passed;

        _entries.AddOrUpdate(
            vehicleId,
            _ => New(latitude, longitude, producedState, nowUtc),
            (_, entry) =>
            {
                var advanced = Advance(entry, latitude, longitude, producedState, nowUtc);
                decision = advanced.Decision;
                return advanced.Entry;
            });

        return decision;
    }

    private static GateEntry New(double lat, double lon, VehicleState state, DateTime now)
        => new(lat, lon, state, LastPassedAt: now, LastSeenAt: now);

    private (GateEntry Entry, GpsGateDecision Decision) Advance(
        GateEntry entry,
        double lat,
        double lon,
        VehicleState producedState,
        DateTime now)
    {
        // Rama 0 — cambio de estado: pasa siempre. Es la trama de la frenada o del
        // arranque, y es la que dispara LongStopDetected / LongStopEnded.
        if (producedState != entry.State)
        {
            return (
                entry with { Lat = lat, Lon = lon, State = producedState, LastPassedAt = now, LastSeenAt = now },
                GpsGateDecision.Passed);
        }

        var suppressedFor = now - entry.LastPassedAt;

        // Rama 1 — detenido: se compara contra LastPassedAt, NO contra el inicio del
        // estado. El tiempo en el estado no vuelve a crecer, así que comparar contra él
        // solo suprime durante los primeros MaxSuppressSegundos y a partir de ahí un
        // vehículo parado volvería a persistir trama a trama. LastPassedAt sí se
        // renueva en cada pasada, que es lo que lo hace funcionar como reloj.
        if (producedState == VehicleState.Detenido && suppressedFor < _maxSuppress)
        {
            return (entry with { LastSeenAt = now }, GpsGateDecision.Suppressed);
        }

        // Rama 2 — moviéndose: si no se ha movido y la ventana sigue abierta, fuera.
        if (_minDistanceMeters > 0
            && DistanceMeters(entry.Lat, entry.Lon, lat, lon) < _minDistanceMeters
            && suppressedFor < _maxSuppress)
        {
            return (entry with { LastSeenAt = now }, GpsGateDecision.Suppressed);
        }

        return (
            entry with { Lat = lat, Lon = lon, LastPassedAt = now, LastSeenAt = now },
            GpsGateDecision.Passed);
    }

    public bool TryClaimHeartbeat(Guid vehicleId, DateTime nowUtc)
    {
        var previous = _lastHeartbeatAt.GetOrAdd(vehicleId, DateTime.MinValue);
        if (nowUtc - previous < _heartbeatWindow) return false;
        return _lastHeartbeatAt.TryUpdate(vehicleId, nowUtc, previous);
    }

    public bool TryClaimHeartbeatBroadcast(DateTime nowUtc)
    {
        var lastTicks = Interlocked.Read(ref _lastBroadcastUtcTicks);
        if (lastTicks != 0 && nowUtc - new DateTime(lastTicks, DateTimeKind.Utc) < _heartbeatWindow) return false;

        return Interlocked.CompareExchange(ref _lastBroadcastUtcTicks, nowUtc.Ticks, lastTicks) == lastTicks;
    }

    /// <summary>
    /// Descarta los vehículos que no envían nada desde hace el doble de la ventana
    /// de supresión. Con MaxTrackedVehicles vehículos o menos no hay nada que purgar,
    /// así que ni se mira el diccionario.
    ///
    /// El contador va con Increment porque con Read el valor nunca cambiaba y el
    /// módulo daba 0, o sea que la purga se ejecutaba en CADA trama en vez de una de
    /// cada 64.
    /// </summary>
    private void PurgeIfNeeded()
    {
        if (_entries.Count <= _maxTrackedVehicles) return;
        if (Interlocked.Increment(ref _purges) % 64 != 0) return;

        var staleBefore = DateTime.UtcNow - (_maxSuppress + _maxSuppress);
        foreach (var (id, entry) in _entries)
        {
            if (entry.LastSeenAt >= staleBefore) continue;
            _entries.TryRemove(id, out _);
            _lastHeartbeatAt.TryRemove(id, out _);
        }
    }

    private static double DistanceMeters(double lat1, double lon1, double lat2, double lon2)
    {
        const double earthRadiusMeters = 6_371_000;
        const double toRadians = Math.PI / 180;

        var dLat = (lat2 - lat1) * toRadians;
        var dLon = (lon2 - lon1) * toRadians;
        var a = Math.Pow(Math.Sin(dLat / 2), 2)
              + Math.Cos(lat1 * toRadians) * Math.Cos(lat2 * toRadians) * Math.Pow(Math.Sin(dLon / 2), 2);

        return 2 * earthRadiusMeters * Math.Asin(Math.Sqrt(Math.Min(1, a)));
    }

    private sealed record GateEntry(
        double Lat,
        double Lon,
        VehicleState State,
        DateTime LastPassedAt,
        DateTime LastSeenAt);
}
