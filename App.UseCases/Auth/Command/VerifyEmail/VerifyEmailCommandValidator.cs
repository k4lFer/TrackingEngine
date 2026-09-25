using System.Net;
using App.Objects.User.DTOs.Input.Command;
using App.Shared.Common.Result;
using App.Shared.Common.Validation;

namespace App.UseCases.Auth.Command.VerifyEmail;

public class VerifyEmailCommandValidator : IInputValidator<VerifyEmailDto>
{
    private readonly List<MessageDto> _messages = [];
    public HttpStatusCode StatusCode { get; private set; }
    public IReadOnlyCollection<MessageDto> Messages => _messages;

    public Task<bool> ValidateAsync(VerifyEmailDto input, CancellationToken cancellationToken = default)
    {
        _messages.Clear();

        if (input is null)
        {
            _messages.Add(new MessageDto(code: "NULL_INPUT", message: "Input data is required."));
            StatusCode = HttpStatusCode.BadRequest;
            return Task.FromResult(false);
        }

        var hasToken = !string.IsNullOrWhiteSpace(input.Token);
        var hasCode = !string.IsNullOrWhiteSpace(input.Code);

        if (!hasToken && !hasCode)
        {
            _messages.Add(new MessageDto(code: "MISSING_VERIFICATION", message: "A token or code is required."));
        }

        if (hasToken && hasCode)
        {
            _messages.Add(new MessageDto(code: "AMBIGUOUS_VERIFICATION", message: "Provide either the token or the code, not both."));
        }

        if (hasCode && string.IsNullOrWhiteSpace(input.Email))
        {
            _messages.Add(new MessageDto(code: "EMAIL_REQUIRED", message: "Email is required when using a code."));
        }

        if (_messages.Any())
        {
            StatusCode = HttpStatusCode.UnprocessableEntity;
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }
}