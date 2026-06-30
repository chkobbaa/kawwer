namespace Kawwer.Application.Common.Messaging;

/// <summary>
/// Sends commands/queries to their handler, applying the validation pipeline first.
/// </summary>
public interface IDispatcher
{
    Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
}
