namespace App.Objects.User.DTOs.Input.Command;

public class VerifyEmailDto
{
    public string? Email { get; set; }
    public string? Token { get; set; }
    public string? Code { get; set; }
}