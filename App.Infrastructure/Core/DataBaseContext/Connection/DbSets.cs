using App.Domain.Vehicles.Entities;
using App.Domain.Geofences.Entities;
using App.Domain.Materials.Entities;
using App.Domain.Notifications.Entities;
using App.Domain.Routes.Entities;
using App.Domain.Tracking.Entities;
using App.Domain.User.Entities;
using App.Domain.Vehicles.Entities;
using App.Infrastructure.Core.DataBaseContext.Audit;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Core.DataBaseContext.Connection;

public partial class AppDataBaseContext
{
    public DbSet<TUser> Users { get; set; }
    public DbSet<TPerson> Persons { get; set; }
    public DbSet<TRefreshToken> RefreshTokens { get; set; }
    public DbSet<TUserGateway> UserGateways { get; set; }
    public DbSet<TEmailVerification> EmailVerifications { get; set; }
    public DbSet<AuditMessage> AuditMessages { get; set; }
    public DbSet<TNotification> Notifications { get; set; }
    public DbSet<TMaterial> Materials { get; set; }
    public DbSet<TRoute> Routes { get; set; }
    public DbSet<TMineRoad> MineRoads { get; set; }
    public DbSet<TGeofence> Geofences { get; set; }
    public DbSet<TTrip> Trips { get; set; }
    public DbSet<TTrackingEvent> TrackingEvents { get; set; }
    public DbSet<TGpsPosition> GpsPositions { get; set; }
    public DbSet<TVehicle> Vehicles { get; set; }
    public DbSet<TVehicleCurrentState> VehicleCurrentStates { get; set; }
    public DbSet<TDevice> Devices { get; set; }
}
