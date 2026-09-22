namespace App.Objects.User.DTOs.Output.Response;

public class LoginResponseDto
{
    public string Token { get; set; }
    public Guid UserId { get; set; }
    public string Email { get; set; }
    public string Username { get; set; }
    public string DeviceId { get; set; }
}
