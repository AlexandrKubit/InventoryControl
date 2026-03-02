namespace App.Base.Mediator;

using Exchange;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;

public static class Mediator
{
    private static readonly ConcurrentDictionary<Type, Type> handlerTypes = new();
    private static readonly ConcurrentDictionary<Type, Type> validatorTypes = new();

    /// <summary>
    /// Автоматическая регистрация обработчиков и валидаторов.
    /// </summary>
    public static void RegisterHandlersAndValidators(Assembly assembly)
    {
        var types = assembly.GetTypes();

        // Регистрируем обработчики
        var assemblyHandlerTypes = types
            .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)));

        foreach (var type in assemblyHandlerTypes)
        {
            var interfaceType = type.GetInterfaces().First(i => i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));
            var requestType = interfaceType.GetGenericArguments()[0];
            handlerTypes[requestType] = type;
        }

        // Регистрируем валидаторы
        var assemblyValidatorTypes = types
            .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestValidator<,>)));

        foreach (var type in assemblyValidatorTypes)
        {
            var interfaceType = type.GetInterfaces().First(i => i.GetGenericTypeDefinition() == typeof(IRequestValidator<,>));
            var requestType = interfaceType.GetGenericArguments()[0];
            validatorTypes[requestType] = type;
        }
    }

    /// <summary>
    /// Генерация маршрутов на основе зарегистрированных запросов.
    /// </summary>
    public static void RegisterEndpoints(WebApplication app)
    {
        foreach (var handlerType in handlerTypes)
        {
            var routeAttribute = handlerType.Value.GetCustomAttribute<RequestRouteAttribute>();
            if (routeAttribute == null) continue;

            var method = app.MapPost(routeAttribute.Path, async (HttpContext context, IServiceProvider provider) =>
            {
                var request = await DeserializeRequestAsync(context, handlerType.Key);
                var response = await ProcessRequestAsync(context, request, handlerType.Key, routeAttribute);
                await WriteResponseAsync(context, response);
            });

            var responseType = handlerType.Key
                .GetInterfaces()
                .First(i => i.GetGenericTypeDefinition() == typeof(IRequest<>))
                .GetGenericArguments()[0];

            method.Produces(StatusCodes.Status200OK, responseType, "application/json");
            method.Accepts(handlerType.Key, "application/json");
        }
    }

    private static async Task<object> ProcessRequestAsync(HttpContext context, IBaseRequest request, Type requestType, RequestRouteAttribute routeAttribute)
    {
        // Выполняем валидацию, если валидатор существует
        if (validatorTypes.TryGetValue(requestType, out var validatorType) && validatorType != null)
        {
            var validator = (IBaseRequestValidator)ActivatorUtilities.CreateInstance(context.RequestServices, validatorType);
            validator.BaseValidate(request);
        }

        // Выполняем запрос через обработчик
        if (!handlerTypes.TryGetValue(requestType, out var handlerType))
            throw new Exception($"Обработчик для запроса {requestType.Name} не зарегистрирован.");

        var handler = (IBaseRequestHandler)ActivatorUtilities.CreateInstance(context.RequestServices, handlerType);
        object result;

        if(routeAttribute.Type == RequestRouteAttribute.Types.Command)
        {
            // для команд нужна транзакция
            var uow = (IUnitOfWork)context.RequestServices.GetService(typeof(IUnitOfWork));
            result = await PrivateHandleAsync(handler, request, uow);
        }
        else
        {
            // для запросов транзакция не нужна
            result = await handler.BaseHandleAsync(request);
        }

        return result;
    }

    private static async Task<IBaseRequest> DeserializeRequestAsync(HttpContext context, Type requestType)
    {
        context.Request.EnableBuffering();
        var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
        context.Request.Body.Position = 0;

        if (string.IsNullOrEmpty(body))
            body = "{}";

        return (IBaseRequest)JsonSerializer.Deserialize(body, requestType, new JsonSerializerOptions(JsonSerializerDefaults.Web))
            ?? throw new Exception($"Не удалось десериализовать запрос типа {requestType.Name}.");
    }

    private static async Task WriteResponseAsync(HttpContext context, object response)
    {
        if (!context.Response.HasStarted)
            context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(response);
    }

    private static async Task<object> PrivateHandleAsync(IBaseRequestHandler handler, IBaseRequest request, IUnitOfWork uow)
    {
        for (int retry = 0; retry < 5; retry++) // Разрешим максимум 5 попыток.
        {
            try
            {
                await uow.InitializeAsync();
                var result = await handler.BaseHandleAsync(request);
                await uow.CommitAsync();
                return result;
            }
            catch (Exception ex)
            {
                await uow.RollbackAsync();
                if (uow.IsDeadlockException(ex))
                {
                    await Task.Delay(retry * 1000);
                }
                else
                {
                    throw; // Пробрасываем другие исключения.
                }
            }
        }

        throw new Exception("Достигнут лимит повторов транзакций.");
    }
}