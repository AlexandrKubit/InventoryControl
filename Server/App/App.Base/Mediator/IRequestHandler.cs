namespace App.Base.Mediator;
using Exchange;

public interface IBaseRequestHandler
{
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