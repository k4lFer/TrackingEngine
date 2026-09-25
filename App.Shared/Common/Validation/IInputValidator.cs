using App.Shared.Common.Result;

namespace App.Shared.Common.Validation;

public interface IInputValidator<in T> : IHttpResponse
{
    IReadOnlyCollection<MessageDto> Messages { get; }
    public Task<bool> ValidateAsync(T input, CancellationToken cancellationToken = default);
}