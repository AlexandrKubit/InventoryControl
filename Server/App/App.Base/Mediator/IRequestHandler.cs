namespace App.Base.Mediator;
using Exchange;

public interface IBaseRequestHandler
{
    // по умолчанию используем Serializable
    // для большинства сценариев в корпоративных системах он остаётся оптимальным
    System.Data.IsolationLevel IsolationLevel => System.Data.IsolationLevel.Serializable;
    Task<object> BaseHandleAsync(IBaseRequest request);
}

public interface IRequestHandler<TRequest, TResponse> : IBaseRequestHandler where TRequest : IRequest<TResponse>
{
    Task<TResponse> HandleAsync(TRequest request);

    async Task<object> IBaseRequestHandler.BaseHandleAsync(IBaseRequest request)
    {
        return await HandleAsync((TRequest)request);
    }
}