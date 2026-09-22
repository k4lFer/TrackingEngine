using App.Objects.User.DTOs.Input.Command;
using App.Shared.Result;
using App.Shared.Validation;

namespace App.UseCases.Auth.Command.Login;

public class LoginCommandValidator : IInputValidator<LoginDto>
{
    private readonly List<MessageDto> _messages = [];
    public System.Net.HttpStatusCode StatusCode { get; private set; }
    public IReadOnlyCollection<MessageDto> Messages => _messages;
    
    public async Task<bool> ValidateAsync(LoginDto input, CancellationToken cancellationToken = default)
    {
        _messages.Clear();
        
        if (input is null)
        {
            _messages.Add(new MessageDto(code: "NULL_INPUT", message: "Input data is required."));
            StatusCode = System.Net.HttpStatusCode.BadRequest;
            return false;
        }
        
        if (string.IsNullOrWhiteSpace(input.Email))
        {
            _messages.Add(new MessageDto(code: "EMAIL_REQUIRED", message: "Email is required."));
        }
        
        if (string.IsNullOrWhiteSpace(input.Password))
        {
            _messages.Add(new MessageDto(code: "PASSWORD_REQUIRED", message: "Password is required."));
        }
        
        if (_messages.Any())
        {
            StatusCode = System.Net.HttpStatusCode.UnprocessableEntity;
            return false;
        }
        
        return true;
    }
}
