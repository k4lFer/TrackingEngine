namespace App.Objects.User.DTOs.Output.Response;

public record ActiveSessionDto (
    
)
{
    public Guid Id { get; set; }
    public string DeviceId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastActivityAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public bool IsCurrentDevice { get; set; }
    public bool IsRevoked => RevokedAt.HasValue;
}