namespace App.Objects.Materials.DTOs.Input.Command;

public class UpdateMaterialRequest
{
    public string Name { get; set; }
    public string Unit { get; set; }
    public bool Active { get; set; }
}