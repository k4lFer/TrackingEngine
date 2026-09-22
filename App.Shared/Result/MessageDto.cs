namespace App.Shared.Result;

public class MessageDto
{
    public string Code { get; init; } = default!;
    public string Message { get; init; } = default!;

    public MessageDto(string message, string code = "GENERIC")
    {
        Message = message;
        Code = code;
    }
}