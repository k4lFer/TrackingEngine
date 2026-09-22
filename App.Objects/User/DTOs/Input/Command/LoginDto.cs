namespace App.Objects.User.DTOs.Input.Command;

public class LoginDto
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string? DeviceId { get; set; }
}
