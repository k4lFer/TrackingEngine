namespace App.Domain.User.Entities;

public class TRefreshToken
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string DeviceId { get; private set; }
    public string TokenHash { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime LastActivityAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    public TUser User { get; private set; }

    private TRefreshToken() { }

    private TRefreshToken(
        Guid userId,
        string deviceId,
        string tokenHash,
        DateTime? expiresAt,
        string? ipAddress,
        string? userAgent)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        DeviceId = deviceId;
        TokenHash = tokenHash;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        ExpiresAt = expiresAt;
        CreatedAt = DateTime.UtcNow;
        LastActivityAt = DateTime.UtcNow;
        RevokedAt = null;
    }

    public static TRefreshToken Create(
        Guid userId,
        string deviceId,
        string tokenHash,
        DateTime? expiresAt,
        string? ipAddress,
        string? userAgent)
    {
        return new TRefreshToken(userId, deviceId, tokenHash, expiresAt, ipAddress, userAgent);
    }

    public void Revoke()
    {
        RevokedAt = DateTime.UtcNow;
    }

    public bool IsActive => RevokedAt is null && (ExpiresAt is null || ExpiresAt > DateTime.UtcNow);

    public bool IsExpiredByInactivity(DateTime now, TimeSpan inactivityWindow)
        => LastActivityAt.Add(inactivityWindow) < now;
}
