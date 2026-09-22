namespace App.Domain.User.Entities;

public class TUserGateway
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Provider { get; private set; }
    public string ProviderId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public TUser User { get; private set; }

    private TUserGateway() { }

    private TUserGateway(Guid userId, string provider, string providerId)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Provider = provider;
        ProviderId = providerId;
        CreatedAt = DateTime.UtcNow;
    }

    public static TUserGateway Create(Guid userId, string provider, string providerId)
    {
        return new TUserGateway(userId, provider, providerId);
    }
}
