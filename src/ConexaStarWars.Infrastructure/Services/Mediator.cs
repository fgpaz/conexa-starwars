using ConexaStarWars.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ConexaStarWars.Infrastructure.Services;

public class Mediator(IServiceProvider serviceProvider, ILogger<Mediator> logger) : IMediator
{
    public async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var requestType = request.GetType();
        var responseType = typeof(TResponse);
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);

        logger.LogDebug("Enviando request {RequestType} esperando respuesta {ResponseType}",
            requestType.Name, responseType.Name);

        var handler = serviceProvider.GetService(handlerType);
        if (handler == null) throw new InvalidOperationException($"No se encontró handler para {requestType.Name}");

        var method = handlerType.GetMethod("HandleAsync");
        if (method == null) throw new InvalidOperationException($"Método HandleAsync no encontrado en {handlerType.Name}");

        try
        {
            var result = await (Task<TResponse>)method.Invoke(handler, new object[] { request, cancellationToken })!;
            logger.LogDebug("Request {RequestType} procesado exitosamente", requestType.Name);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error procesando request {RequestType}", requestType.Name);
            throw;
        }
    }

    public async Task SendAsync(IRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var requestType = request.GetType();
        var handlerType = typeof(IRequestHandler<>).MakeGenericType(requestType);

        logger.LogDebug("Enviando comando {RequestType}", requestType.Name);

        var handler = serviceProvider.GetService(handlerType);
        if (handler == null) throw new InvalidOperationException($"No se encontró handler para {requestType.Name}");

        var method = handlerType.GetMethod("HandleAsync");
        if (method == null) throw new InvalidOperationException($"Método HandleAsync no encontrado en {handlerType.Name}");

        try
        {
            await (Task)method.Invoke(handler, new object[] { request, cancellationToken })!;
            logger.LogDebug("Comando {RequestType} procesado exitosamente", requestType.Name);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error procesando comando {RequestType}", requestType.Name);
            throw;
        }
    }

    public async Task PublishAsync<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        if (notification == null)
            throw new ArgumentNullException(nameof(notification));

        var notificationType = typeof(TNotification);
        var handlerType = typeof(INotificationHandler<>).MakeGenericType(notificationType);

        logger.LogDebug("Publicando notificación {NotificationType}", notificationType.Name);

        var handlers = serviceProvider.GetServices(handlerType);
        var tasks = new List<Task>();

        foreach (var handler in handlers)
        {
            var method = handlerType.GetMethod("HandleAsync");
            if (method != null)
            {
                var task = (Task)method.Invoke(handler, new object[] { notification, cancellationToken })!;
                tasks.Add(task);
            }
        }

        try
        {
            await Task.WhenAll(tasks);
            logger.LogDebug("Notificación {NotificationType} procesada por {HandlerCount} handlers",
                notificationType.Name, tasks.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error procesando notificación {NotificationType}", notificationType.Name);
            throw;
        }
    }
}