using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;

namespace Kawwer.Application.Common.Messaging;

/// <summary>
/// Resolves the handler for a request from the DI container and runs any registered
/// FluentValidation validators before invoking it.
/// </summary>
public sealed class Dispatcher : IDispatcher
{
    private readonly IServiceProvider _provider;

    public Dispatcher(IServiceProvider provider)
    {
        _provider = provider;
    }

    public async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        await ValidateAsync(request, cancellationToken);

        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));
        var handler = _provider.GetService(handlerType)
                      ?? throw new InvalidOperationException($"No handler registered for {request.GetType().Name}.");

        var method = handlerType.GetMethod("HandleAsync")!;
        var task = (Task<TResponse>)method.Invoke(handler, new object[] { request, cancellationToken })!;
        return await task;
    }

    private async Task ValidateAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken)
    {
        var validatorType = typeof(IValidator<>).MakeGenericType(request.GetType());
        var validators = _provider.GetServices(validatorType).Cast<IValidator>().ToList();
        if (validators.Count == 0)
        {
            return;
        }

        var context = new ValidationContext<object>(request);
        var failures = new List<ValidationFailure>();
        foreach (var validator in validators)
        {
            var result = await validator.ValidateAsync(context, cancellationToken);
            if (!result.IsValid)
            {
                failures.AddRange(result.Errors);
            }
        }

        if (failures.Count > 0)
        {
            throw new Kawwer.Application.Common.Exceptions.ValidationException(failures);
        }
    }
}
