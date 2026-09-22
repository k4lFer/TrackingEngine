using App.Shared.Objects.Enums;

namespace App.Domain.User.Entities;

public class TEmailVerification
{
    public const int MaxFailedAttempts = 5;

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Email { get; private set; }
    public string TokenHash { get; private set; }
    public string CodeHash { get; private set; }
    public VerificationType Type { get; private set; }
    public DateTime LinkExpiresAt { get; private set; }
    public DateTime CodeExpiresAt { get; private set; }
    public DateTime? UsedAt { get; private set; }
    public int FailedAttempts { get; private set; }
    public bool IsValid { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private TEmailVerification() { }

    private TEmailVerification(
        Guid userId,
        string email,
        string tokenHash,
        string codeHash,
        VerificationType type,
        DateTime linkExpiresAt,
        DateTime codeExpiresAt)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Email = email;
        TokenHash = tokenHash;
        CodeHash = codeHash;
        Type = type;
        LinkExpiresAt = linkExpiresAt;
        CodeExpiresAt = codeExpiresAt;
        UsedAt = null;
        FailedAttempts = 0;
        IsValid = true;
        CreatedAt = DateTime.UtcNow;
    }

    public static TEmailVerification Create(
        Guid userId,
        string email,
        string tokenHash,
        string codeHash,
        VerificationType type,
        DateTime linkExpiresAt,
        DateTime codeExpiresAt)
    {
        return new TEmailVerification(userId, email, tokenHash, codeHash, type, linkExpiresAt, codeExpiresAt);
    }

    public bool IsLinkUsable => IsValid && UsedAt is null && LinkExpiresAt > DateTime.UtcNow;

    public bool IsCodeUsable => IsValid && UsedAt is null && CodeExpiresAt > DateTime.UtcNow;

    public bool HasExceededAttempts => FailedAttempts >= MaxFailedAttempts;

    public void RegisterFailedAttempt()
    {
        FailedAttempts++;
    }

    public void MarkUsed()
    {
        if (!IsValid) return;
        IsValid = false;
        UsedAt = DateTime.UtcNow;
    }

    public void Invalidate()
    {
        IsValid = false;
    }
}