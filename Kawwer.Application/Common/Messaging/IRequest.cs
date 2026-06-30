namespace Kawwer.Application.Common.Messaging;

/// <summary>Marker for a CQRS command or query that returns a response.</summary>
public interface IRequest<TResponse>
{
}
