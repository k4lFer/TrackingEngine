namespace App.Domain.User.Entities;

public class TPerson
{
    public Guid Id { get; private set; }

    public Guid UserId { get; private set; } 

    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public DateOnly DateOfBirth { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private TPerson() { } // EF Core

    private TPerson(string firstName, string lastName, DateOnly dateOfBirth)
    {
        Id = Guid.NewGuid();
        FirstName = firstName;
        LastName = lastName;
        DateOfBirth = dateOfBirth;

        CreatedAt = DateTime.UtcNow;
        UpdatedAt = null;
    }

    internal static TPerson Create(string firstName, string lastName, DateOnly dateOfBirth)
    {
        return new TPerson(firstName, lastName, dateOfBirth);
    }
}