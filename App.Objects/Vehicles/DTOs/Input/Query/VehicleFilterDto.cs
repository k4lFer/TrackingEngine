using App.Shared.Query;

namespace App.Objects.Vehicles.DTOs.Input.Query;

public class VehicleFilterDto : QueryDto
{
    public VehicleFilterDto() : base(pageSize: 500)
    {
    }

    public string? Search { get; set; }

    public string? State { get; set; }

    /// <summary>true = solo vehículos en línea (excluye Offline y FueraDeServicio).</summary>
    public bool? OnlyOnline { get; set; }
}