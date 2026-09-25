namespace App.Shared.Common.Query;

public class QueryDto
{
    public int NumberPage { get; set; }
    public int PageSize { get; set; }

    public QueryDto() : this(pageSize: 10)
    {
    }

    /// <summary>
    /// Permite a los DTOs de listado fijar un PageSize por defecto propio
    /// (p. ej. 500 para los listados que alimentan mapas y selects completos).
    /// </summary>
    protected QueryDto(int pageSize)
    {
        NumberPage = 1;
        PageSize = pageSize;
    }
}