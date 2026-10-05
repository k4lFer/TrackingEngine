using System.Collections.Concurrent;
using App.Interfaces.Ports.Tracking;
using App.Shared.Common.Enums;
using App.Shared.Common.Gps;
using Microsoft.Extensions.Options;

namespace App.UseCases.Gps;

public sealed class VehicleRealtimeGate : IVehicleRealtimeGate
{
    private readonly ConcurrentDictionary<Guid, GateEntry> _entries = new();
    private readonly ConcurrentDictionary<Guid, DateTime> _lastHeartbeatAt = new();
    private readonly decimal _stopSpeedKmh;
    private readonly double _minDistanceMeters;
    private readonly TimeSpan _maxSuppress;
    private readonly TimeSpan _minStateDuration;
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
        _minStateDuration = TimeSpan.FromSeconds(Math.Max(0, o.StateMinSeconds));
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
        => new(lat, lon, state, LastPassedAt: now, LastSeenAt: now,
               PendingState: state, PendingSince: now);

    private (GateEntry Entry, GpsGateDecision Decision) Advance(
        GateEntry entry,
        double lat,
        double lon,
        VehicleState producedState,
        DateTime now)
    {
        if (producedState != entry.State)
        {
            var pendingSince = entry.PendingState == producedState ? entry.PendingSince : now;

            if (now - pendingSince < _minStateDuration)
            {
                return (
                    entry with { LastSeenAt = now, PendingState = producedState, PendingSince = pendingSince },
                    GpsGateDecision.Suppressed);
            }

            return (
                entry with
                {
                    Lat = lat,
                    Lon = lon,
                    State = producedState,
                    PendingState = producedState,
                    PendingSince = now,
                    LastPassedAt = now,
                    LastSeenAt = now
                },
                GpsGateDecision.Passed);
        }

        var settled = entry with { LastSeenAt = now, PendingState = producedState, PendingSince = now };
        var suppressedFor = now - entry.LastPassedAt;

        if (entry.State == VehicleState.Detenido && suppressedFor < _maxSuppress)
        {
            return (settled, GpsGateDecision.Suppressed);
        }

        if (_minDistanceMeters > 0
            && DistanceMeters(entry.Lat, entry.Lon, lat, lon) < _minDistanceMeters
            && suppressedFor < _maxSuppress)
        {
            return (settled, GpsGateDecision.Suppressed);
        }

        return (
            settled with { Lat = lat, Lon = lon, LastPassedAt = now },
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
        DateTime LastSeenAt,
        VehicleState PendingState,
        DateTime PendingSince);
}
