namespace App.Objects.Materials.DTOs.Input.Command;

public class CreateMaterialRequest
{
    public string Code { get; set; }
    public string Name { get; set; }
    public string Unit { get; set; }
}