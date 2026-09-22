namespace App.Interfaces.Ports;

public interface IUnitOfWork
{
    Task<int> SaveChanges(CancellationToken cancellationToken = default);
}