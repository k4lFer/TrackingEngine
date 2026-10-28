using App.Infrastructure.Adapters.Messaging.Redis;
using GpsGateway.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<RedisGpsStreamOptions>(builder.Configuration.GetSection(RedisGpsStreamOptions.SectionName));
builder.Services.AddRedisMessaging(builder.Configuration);

builder.Services.AddHostedService<GpsTcpGatewayHostedService>();

var host = builder.Build();
host.Run();
