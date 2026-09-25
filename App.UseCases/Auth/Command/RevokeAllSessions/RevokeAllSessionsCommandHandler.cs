using System.Net;
using App.Interfaces.Ports;
using App.Interfaces.Ports.User;
using App.Shared.Common.Result;
using Cortex.Mediator.Commands;

namespace App.UseCases.Auth.Command.RevokeAllSessions;

public class RevokeAllSessionsCommandHandler : ICommandHandler<RevokeAllSessionsCommand, OutputPort<object>>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RevokeAllSessionsCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<OutputPort<object>> Handle(RevokeAllSessionsCommand command, CancellationToken cancellationToken)
    {
        await _refreshTokenRepository.RevokeAllUserTokensAsync(command.UserId, command.DeviceId, cancellationToken);
        await _unitOfWork.SaveChanges(cancellationToken);

        return OutputPort<object>.Success(
            data: null,
            statusCode: HttpStatusCode.OK,
            message: "All sessions revoked successfully"
        );
    }
}