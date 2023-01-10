using MediatR;
using OutlayApp.Domain.Shared;

namespace OutlayApp.Application.Abstractions.Messaging;

public interface ICommand : IRequest<Result>
{
}

public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}