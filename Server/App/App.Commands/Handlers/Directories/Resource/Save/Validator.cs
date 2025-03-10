namespace App.Commands.Handlers.Directories.Resource.Save;

using Exchange.Commands.Directories.Resource.Save;
using App.Base.Mediator;
using Common.Exceptions;

public class Validator : IRequestValidator<Request, Guid>
{
    public void Validate(Request request)
    {
        if (string.IsNullOrEmpty(request.Name.Trim()))
            throw new ValidationException("Не указано наименование");
    }
}
