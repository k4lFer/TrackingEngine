using App.Domain.User.Events;
using App.Shared.Domain;

namespace App.Domain.User.Entities;

public class TUser : BaseDomain
{
    public string Email { get; private set; }
    public string Username { get; private set; }
    public string? Password { get; private set; }
    public bool IsEmailVerified { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public TPerson Person { get; private set; }

    public ICollection<TRefreshToken> RefreshTokens { get; private set; } = new List<TRefreshToken>();  
    public ICollection<TUserGateway> UserGateways { get; private set; } = new List<TUserGateway>();

    private IReadOnlyCollection<TRefreshToken> _refreshTokens => RefreshTokens.ToList().AsReadOnly();
    private IReadOnlyCollection<TUserGateway> _userGateways => UserGateways.ToList().AsReadOnly();

    private TUser() { } // EF Core

    private TUser(string email, string username, string password, bool isEmailVerified, TPerson person)
    {
        Email = email;
        Username = username;
        Password = password;
        IsEmailVerified = isEmailVerified;
        Person = person;

        CreatedAt = DateTime.UtcNow;
        UpdatedAt = null;

    }

    public static TUser Create(
        string email,
        string username,
        string password,
        string firstName,
        string lastName,
        DateOnly dateOfBirth
    )
    {
        var person = TPerson.Create(firstName, lastName, dateOfBirth);
        
        var user = new TUser(email, username, password, false, person);
        
        user.AddDomainEvent(new UserCreatedEvent(user.Id, email, username, user.IsEmailVerified));
        
        return user;
    }

    public void MarkEmailVerified()
    {
        IsEmailVerified = true;
        UpdatedAt = DateTime.UtcNow;
    }
}