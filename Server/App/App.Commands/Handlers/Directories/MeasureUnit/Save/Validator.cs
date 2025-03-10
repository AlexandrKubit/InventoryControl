namespace App.Commands.Handlers.Directories.MeasureUnit.Save;

using Exchange.Commands.Directories.MeasureUnit.Save;
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
