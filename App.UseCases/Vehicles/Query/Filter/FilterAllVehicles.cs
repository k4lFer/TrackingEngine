using App.Objects.Vehicles.DTOs.Output.Response;
using App.Shared.Common.Enums;
using App.Shared.Common.Query;

namespace App.UseCases.Vehicles.Query.Filter;

public class FilterAllVehicles : QueryFilter<VehicleStatusResponse>
{
    private static readonly HashSet<string> OnlineStates = Enum.GetValues<VehicleState>()
        .Where(s => s != VehicleState.Offline && s != VehicleState.FueraDeServicio)
        .Select(s => Normalize(s.ToString()))
        .ToHashSet();

    private static string Normalize(string value) =>
        value.Replace("_", string.Empty).ToLowerInvariant();

    public string? Search { get; set; }

    public string? State { get; set; }

    public bool? OnlyOnline { get; set; }

    public override IQueryable<VehicleStatusResponse> ApplyFilter(IQueryable<VehicleStatusResponse> query)
    {
        if (!string.IsNullOrWhiteSpace(Search))
        {
            var search = Search.Trim();
            query = query.Where(v =>
                v.Code.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                v.Plate.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(State))
        {
            var state = State.Trim();
            query = query.Where(v => v.State.Equals(state, StringComparison.OrdinalIgnoreCase));
        }

        if (OnlyOnline == true)
        {
            query = query.Where(v => OnlineStates.Contains(Normalize(v.State)));
        }

        return query;
    }
}