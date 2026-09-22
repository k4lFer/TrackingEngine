using App.Shared.Domain;
using App.Shared.Objects.Enums;
using NetTopologySuite.Geometries;

namespace App.Domain.Vehicles.Events;

public sealed record VehiclePositionReportedEvent(Guid VehicleId, Point Geom, VehicleState State) : BaseEvent;
