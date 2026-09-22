using System.Text.Json;
using System.Text.Json.Serialization;
using NetTopologySuite.Geometries;

namespace App.Infrastructure.Core.DataBaseContext.Audit;

public sealed class AuditMessage
{
    private const JsonNumberHandling NonFiniteHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        NumberHandling = NonFiniteHandling
    };
    static AuditMessage()
    {
        SerializerOptions.Converters.Add(new PointToWktConverter());
        SerializerOptions.Converters.Add(new GeometryToWktConverter());
    }

    private sealed class PointToWktConverter : JsonConverter<Point>
    {
        public override Point? Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            throw new NotSupportedException("Deserialization of audit geometry is not supported.");
        }

        public override void Write(
            Utf8JsonWriter writer,
            Point value,
            JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToText());
        }
    }

    private sealed class GeometryToWktConverter : JsonConverter<Geometry>
    {
        public override Geometry? Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            throw new NotSupportedException("Deserialization of audit geometry is not supported.");
        }

        public override void Write(
            Utf8JsonWriter writer,
            Geometry value,
            JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToText());
        }
    }

    public Guid Id { get; private set; }
    public string Type { get; private set; } = null!;
    public string Content { get; private set; } = null!;
    public DateTime OccurredAtUtc { get; private set; }
    public string? Error { get; set; }

    private AuditMessage() { }

    public AuditMessage(object domainEvent)
    {
        Id = Guid.NewGuid();
        Type = domainEvent.GetType().AssemblyQualifiedName!;
        Content = JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), SerializerOptions);
        OccurredAtUtc = DateTime.UtcNow;
    }
}
