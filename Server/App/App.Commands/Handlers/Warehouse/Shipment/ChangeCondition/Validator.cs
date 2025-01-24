namespace App.Commands.Handlers.Warehouse.Shipment.ChangeCondition;

using Exchange.Commands.Warehouse.Shipment.ChangeCondition;
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
