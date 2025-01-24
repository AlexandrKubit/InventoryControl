namespace App.Base.Mediator;

public interface IBaseRequest
{
    System.Data.IsolationLevel IsolationLevel => System.Data.IsolationLevel.Serializable;
}

public interface IRequest<TResponse> : IBaseRequest
{
}
