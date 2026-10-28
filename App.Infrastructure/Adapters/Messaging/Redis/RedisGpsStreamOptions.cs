namespace App.Infrastructure.Adapters.Messaging.Redis;

public sealed class RedisGpsStreamOptions
{
    public const string SectionName = "Redis";
    public string Configuration { get; set; } = "localhost:6379";
    public string StreamKey { get; set; } = "gps:positions:stream";
    public string ConsumerGroup { get; set; } = "app-consumers";
    public int MaxLength { get; set; } = 100000;
}
