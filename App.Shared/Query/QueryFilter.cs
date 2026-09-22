namespace App.Shared.Query;

public abstract class QueryFilter<T>
{
    public abstract IQueryable<T> ApplyFilter(IQueryable<T> query);
}