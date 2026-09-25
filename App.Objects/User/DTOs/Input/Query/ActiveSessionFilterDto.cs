using App.Shared.Common.Enums;
using App.Shared.Common.Query;

namespace App.Objects.User.DTOs.Input.Query;

public class ActiveSessionFilterDto : QueryDto
{
    public SessionStatusEnum Status { get; set; } = SessionStatusEnum.Active;
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? DeviceId { get; set; }
}