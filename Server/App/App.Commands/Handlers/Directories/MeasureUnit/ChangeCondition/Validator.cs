﻿namespace App.Commands.Handlers.Directories.MeasureUnit.ChangeCondition;

using Exchange.Commands.Directories.MeasureUnit.ChangeCondition;
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
