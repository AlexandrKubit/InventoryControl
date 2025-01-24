namespace App.Commands.Handlers.Directories.Client.Delete;

using Exchange.Commands.Directories.Client.Delete;
using App.Base.Mediator;
using Common.Exceptions;

public class Validator : IRequestValidator<Request, Guid>
{
    public void Validate(Request request)
    {
        if (request.Guid == Guid.Empty)
            throw new ValidationException("Неправильный идентификатор");
    }
}
