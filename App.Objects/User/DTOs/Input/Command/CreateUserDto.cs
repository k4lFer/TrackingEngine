namespace App.Objects.User.DTOs.Input.Command;

public class CreateUserDto
{
    public string Email { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DateOfBirth { get; set; }
}