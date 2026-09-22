using System.Net;
using App.Interfaces.Ports;
using App.Interfaces.Ports.User;
using App.Shared.Result;
using Cortex.Mediator.Commands;

namespace App.UseCases.Auth.Command.RevokeSession;

public class RevokeSessionCommandHandler : ICommandHandler<RevokeSessionCommand, OutputPort<object>>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RevokeSessionCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<OutputPort<object>> Handle(RevokeSessionCommand command, CancellationToken cancellationToken)
    {
        await _refreshTokenRepository.RevokeTokenAsync(command.SessionId, cancellationToken);
        await _unitOfWork.SaveChanges(cancellationToken);

        return OutputPort<object>.Success(
            data: null,
            statusCode: HttpStatusCode.OK,
            message: "Session revoked successfully"
        );
    }
}