using App.Shared.Query;

namespace App.Objects.User.DTOs.Input.Query;

public class GetAllDto : QueryDto
{
    public string? Email { get; }
}