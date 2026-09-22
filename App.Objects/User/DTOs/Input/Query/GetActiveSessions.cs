using App.Shared.Query;

namespace App.Objects.User.DTOs.Input.Query;

public class GetActiveSessions : QueryDto
{
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
}