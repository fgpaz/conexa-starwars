namespace ConexaStarWars.Core.Interfaces;

// Interfaz principal del Mediator
public interface IMediator
{
    Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
    Task SendAsync(IRequest request, CancellationToken cancellationToken = default);

    Task PublishAsync<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification;
}

// Interfaces para requests con respuesta
public interface IRequest<out TResponse>
{
}

// Interfaces para requests sin respuesta (comandos)
public interface IRequest
{
}

// Interface para notificaciones (eventos)
public interface INotification
{
}

// Handlers para requests con respuesta
public interface IRequestHandler<in TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken = default);
}

// Handlers para requests sin respuesta
public interface IRequestHandler<in TRequest> where TRequest : IRequest
{
    Task HandleAsync(TRequest request, CancellationToken cancellationToken = default);
}

// Handlers para notificaciones (pueden ser múltiples)
public interface INotificationHandler<in TNotification> where TNotification : INotification
{
    Task HandleAsync(TNotification notification, CancellationToken cancellationToken = default);
}