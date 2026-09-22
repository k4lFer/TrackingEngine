using System.Net;
using App.Objects.User.DTOs.Input.Command;
using App.Shared.Result;
using App.Shared.Validation;

namespace App.UseCases.Auth.Command.ResendVerification;

public class ResendVerificationCommandValidator : IInputValidator<ResendVerificationDto>
{
    private readonly List<MessageDto> _messages = [];
    public HttpStatusCode StatusCode { get; private set; }
    public IReadOnlyCollection<MessageDto> Messages => _messages;

    public Task<bool> ValidateAsync(ResendVerificationDto input, CancellationToken cancellationToken = default)
    {
        _messages.Clear();

        if (input is null)
        {
            _messages.Add(new MessageDto(code: "NULL_INPUT", message: "Input data is required."));
            StatusCode = HttpStatusCode.BadRequest;
            return Task.FromResult(false);
        }

        if (string.IsNullOrWhiteSpace(input.Email))
        {
            _messages.Add(new MessageDto(code: "EMAIL_REQUIRED", message: "Email is required."));
        }

        if (_messages.Any())
        {
            StatusCode = HttpStatusCode.UnprocessableEntity;
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }
}