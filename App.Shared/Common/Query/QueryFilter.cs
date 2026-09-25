namespace App.Shared.Common.Query;

public abstract class QueryFilter<T>
{
    public abstract IQueryable<T> ApplyFilter(IQueryable<T> query);
}