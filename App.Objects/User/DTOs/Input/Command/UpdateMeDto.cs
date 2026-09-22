namespace App.Objects.User.DTOs.Input;

public class UpdateMeDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateOnly DateOfBirth { get; set; } 
}