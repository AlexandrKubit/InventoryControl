﻿namespace App.Commands.Handlers.Warehouse.Shipment.Delete;

using Exchange.Commands.Warehouse.Shipment.Delete;
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
