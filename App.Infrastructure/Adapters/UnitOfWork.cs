using App.Infrastructure.Core.DataBaseContext.Connection;
using App.Interfaces.Ports;

namespace App.Infrastructure.Adapters;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDataBaseContext _context;

    public UnitOfWork(AppDataBaseContext context)
    {
        _context = context;
    } 
    
    public async Task<int> SaveChanges(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}