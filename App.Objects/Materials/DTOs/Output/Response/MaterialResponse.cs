namespace App.Objects.Materials.DTOs.Output.Response;

public record MaterialResponse(
    Guid Id,
    string Code,
    string Name,
    string Unit,
    bool Active
);