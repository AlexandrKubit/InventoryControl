namespace App.Base.Mediator;

public interface IBaseRequestValidator
{
    void BaseValidate(IBaseRequest request);
}

public interface IRequestValidator<TRequest, TResponse> : IBaseRequestValidator where TRequest : IRequest<TResponse>
{
    void Validate(TRequest request);

    void IBaseRequestValidator.BaseValidate(IBaseRequest request)
    {
        Validate((TRequest)request);
    }
}