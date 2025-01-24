namespace App.Base.Mediator;

public interface IBaseRequestHandler
{
    Task<object> BaseHandleAsync(IBaseRequest request, IServiceProvider provider);
}

public interface IRequestHandler<TRequest, TResponse> : IBaseRequestHandler where TRequest : IRequest<TResponse>
{
    Task<TResponse> HandleAsync(TRequest request, IServiceProvider provider);

    async Task<object> IBaseRequestHandler.BaseHandleAsync(IBaseRequest request, IServiceProvider provider)
    {
        return await HandleAsync((TRequest)request, provider);
    }
}