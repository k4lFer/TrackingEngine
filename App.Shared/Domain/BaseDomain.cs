using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace App.Shared.Domain;

public abstract class BaseDomain
{
    private readonly List<BaseEvent> _domainEvents = [];

    public Guid Id { get; protected set; } = Guid.NewGuid();
    
    [NotMapped]
    [JsonIgnore]
    public IReadOnlyCollection<BaseEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(BaseEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    public void RemoveDomainEvent(BaseEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }
}