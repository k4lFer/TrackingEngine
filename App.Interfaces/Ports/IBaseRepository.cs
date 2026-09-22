namespace App.Interfaces.Ports;

public interface IBaseRepository<in TDomain> where TDomain : class
{
    Task AddAsync(TDomain domain,  CancellationToken cancellationToken = default);
}