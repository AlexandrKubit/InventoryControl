﻿namespace App.Commands.Handlers.Directories.Client.Save;

using Exchange.Commands.Directories.Client.Save;
using App.Base.Mediator;
using Common.Exceptions;

public class Validator : IRequestValidator<Request, Guid>
{
    public void Validate(Request request)
    {
        if (string.IsNullOrEmpty(request.Address))
            throw new ValidationException("Не указан адресс");

        if (string.IsNullOrEmpty(request.Name))
            throw new ValidationException("Не указано имя");
    }
}
