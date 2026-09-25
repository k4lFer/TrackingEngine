using App.Objects.User.DTOs.Output.Response;
using App.Shared.Common.Enums;
using App.Shared.Common.Query;

namespace App.UseCases.Auth.Filter;

public class FilterActiveSessions : QueryFilter<ActiveSessionDto>
{
    public SessionStatusEnum Status { get; set; } = SessionStatusEnum.Active;
    public DateTime? InactivityCutoffAt { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? DeviceId { get; set; }

    public override IQueryable<ActiveSessionDto> ApplyFilter(IQueryable<ActiveSessionDto> query)
    {
        var now = DateTime.UtcNow;

        query = Status switch
        {
            SessionStatusEnum.Revoked => query.Where(s => s.RevokedAt != null),
            SessionStatusEnum.All => query,
            _ => query.Where(s => s.RevokedAt == null
                && (s.ExpiresAt == null || s.ExpiresAt > now)
                && (!InactivityCutoffAt.HasValue || s.LastActivityAt >= InactivityCutoffAt.Value))
        };

        if (FromDate.HasValue)
            query = query.Where(s => s.CreatedAt >= FromDate.Value);

        if (ToDate.HasValue)
            query = query.Where(s => s.CreatedAt <= ToDate.Value);

        if (!string.IsNullOrWhiteSpace(DeviceId))
            query = query.Where(s => s.DeviceId == DeviceId);

        return query;
    }
}