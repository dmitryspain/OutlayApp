using MediatR;
using OutlayApp.Domain.Shared;

namespace OutlayApp.Application.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}