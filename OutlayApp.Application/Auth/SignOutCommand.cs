using OutlayApp.Application.Abstractions.Messaging;
using OutlayApp.Domain.Repositories;
using OutlayApp.Domain.Shared;

namespace OutlayApp.Application.Auth;

/// <summary>Revokes the session the request was made with.</summary>
public sealed record SignOutCommand(string AccessToken) : ICommand;

public class SignOutCommandHandler : ICommandHandler<SignOutCommand>
{
    private readonly IClientSessionRepository _sessions;
    private readonly IUnitOfWork _unitOfWork;

    public SignOutCommandHandler(IClientSessionRepository sessions, IUnitOfWork unitOfWork)
    {
        _sessions = sessions;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(SignOutCommand request, CancellationToken cancellationToken)
    {
        var session = await _sessions.GetByTokenHash(TokenHash.Of(request.AccessToken), cancellationToken);
        if (session is not null)
        {
            _sessions.Remove(session);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        return Result.Success();
    }
}
