using App.Shared.Common.Domain;
using App.Shared.Common.Enums;
using NetTopologySuite.Geometries;

namespace App.Domain.Vehicles.Events;

public sealed record VehiclePositionReportedEvent(Guid VehicleId, Point Geom, VehicleState State) : BaseEvent;
