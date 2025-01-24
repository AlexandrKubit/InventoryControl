namespace App.Commands.Handlers.Warehouse.Receipt.Save;

using Exchange.Commands.Warehouse.Receipt.Save;
using App.Base.Mediator;
using Common.Exceptions;

public class Validator : IRequestValidator<Request, Guid>
{
    public void Validate(Request request)
    {
        if (string.IsNullOrEmpty(request.Number))
            throw new ValidationException("Не указан номер");

        if (request.Date == DateTime.MinValue)
            throw new ValidationException("Не указана дата");
    }
}
