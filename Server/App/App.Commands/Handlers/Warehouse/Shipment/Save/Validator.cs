namespace App.Commands.Handlers.Warehouse.Shipment.Save;

using Exchange.Commands.Warehouse.Shipment.Save;
using App.Base.Mediator;
using Common.Exceptions;

public class Validator : IRequestValidator<Request, Guid>
{
    public void Validate(Request request)
    {
        if (string.IsNullOrEmpty(request.Number.Trim()))
            throw new ValidationException("Не указан номер");

        if (request.Date == DateTime.MinValue)
            throw new ValidationException("Не указана дата");

        if (request.ClientGuid == Guid.Empty)
            throw new ValidationException("Не указан клиент");

        if (request.Items.Count <= 0 
            || request.Items.Any(x => x.ResourceGuid == Guid.Empty) 
            || request.Items.Any(x => x.MeasureUnitGuid == Guid.Empty) 
            || request.Items.Any(x => x.Quantity <= 0))
            throw new ValidationException("Неправильно заполнены ресурсы");
    }
}
