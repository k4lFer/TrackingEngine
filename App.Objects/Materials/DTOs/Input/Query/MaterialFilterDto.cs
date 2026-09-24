using App.Shared.Query;

namespace App.Objects.Materials.DTOs.Input.Query;

public class MaterialFilterDto : QueryDto
{
    public MaterialFilterDto() : base(pageSize: 20)
    {
    }

    public string? Search { get; set; }

    public bool? Active { get; set; }
}