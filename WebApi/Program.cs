using App.Infrastructure;
using App.Infrastructure.Core.DataBaseContext.Connection;
using App.Infrastructure.Core.DataBaseContext.Seed;
using App.Interfaces.Ports.Tracking;
using App.UseCases;
using WebApi.Config;
using WebApi.Hubs;
using WebApi.Notifications;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using WebApi.Scalar;
using App.Shared.Security;
using WebApi;
using WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSecurityConfiguration(builder.Configuration, builder.Environment);
builder.Services.AddScalarConfiguration();
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendDev", policy =>
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

builder.Services.AddScoped<ICurrentUser, CurrentUser>();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddUseCasesDi();

builder.Services.AddSingleton<ITrackingNotifier, SignalRTrackingNotifier>();

builder.Services.AddHostedService<GpsTcpGatewayHostedService>();

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDataBaseContext>();
    dbContext.Database.Migrate();
    await DataSeeder.InitializeAsync(dbContext);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseCors("FrontendDev");
app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();
app.MapControllers();
app.MapHub<TrackingHub>("/hubs/tracking");
app.Run();