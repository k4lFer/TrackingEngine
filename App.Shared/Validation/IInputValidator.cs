using App.Shared.Result;

namespace App.Shared.Validation;

public interface IInputValidator<in T> : IHttpResponse
{
    IReadOnlyCollection<MessageDto> Messages { get; }
    public Task<bool> ValidateAsync(T input, CancellationToken cancellationToken = default);
}