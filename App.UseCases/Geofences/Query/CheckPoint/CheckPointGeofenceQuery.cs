using App.Objects.Geofences.DTOs.Output.Response;
using App.Shared.Result;
using Cortex.Mediator.Queries;

namespace App.UseCases.Geofences.Query.CheckPoint;

public class CheckPointGeofenceQuery : IQuery<OutputPort<GeofenceResponse>>
{
    public double Lat { get; }
    public double Lon { get; }

    public CheckPointGeofenceQuery(double lat, double lon)
    {
        Lat = lat;
        Lon = lon;
    }
}