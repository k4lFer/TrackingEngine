using App.Infrastructure.Core.DataBaseContext.Connection;
using App.Interfaces.Ports;
using App.Shared.Query;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Adapters;

public abstract class BaseRepository<TDomain> : IBaseRepository<TDomain> where TDomain : class  
{
    protected readonly AppDataBaseContext _dbc;
    
    public BaseRepository(AppDataBaseContext dbc)
    {
        _dbc = dbc;
    }
    
    protected async Task<QueryResult<T>> PaginateAsync<T>(
        IQueryable<T> query,
        int? pageNumber,
        int? pageSize,
        CancellationToken ct = default)
        where T : class
    {
        var validPageNumber = (pageNumber ?? 1) < 1 ? 1 : (pageNumber ?? 1);
        var validPageSize = (pageSize ?? 10) <= 0 ? 10 : (pageSize ?? 10);

        var totalCount = await query.CountAsync(ct);

        var totalPages = (int)Math.Ceiling(totalCount / (double)validPageSize);

        var items = await query
            .Skip((validPageNumber - 1) * validPageSize)
            .Take(validPageSize)
            .AsNoTracking()
            .ToListAsync(ct);

        return QueryResult<T>.Success(
            results: items,
            totalCount: totalCount,
            totalPages: totalPages,
            pageNumber: validPageNumber,
            pageSize: validPageSize
        );
    }

    public async Task AddAsync(TDomain domain, CancellationToken cancellationToken = default)
    {
        await _dbc.Set<TDomain>().AddAsync(domain, cancellationToken);
    }
}
