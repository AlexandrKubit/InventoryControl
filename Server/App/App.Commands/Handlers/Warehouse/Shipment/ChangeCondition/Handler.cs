namespace App.Commands.Handlers.Warehouse.Shipment.ChangeCondition;

using App.Base.Mediator;
using Domain.Base;
using Domain.Entities.Warehouse.Shipment;
using Exchange.Commands.Warehouse.Shipment.ChangeCondition;
using System.Threading.Tasks;

[RequestRoute("/Warehouse/Shipment/ChangeCondition", RequestRouteAttribute.Types.Command)]
public class Handler(IData data) : IRequestHandler<Request, Guid>
{
    public async Task<Guid> HandleAsync(Request request)
    {
        await data.Shipment.FillByGuids([request.Guid]);

        var shipment = data.Shipment.List.FirstOrDefault(x => x.Guid == request.Guid);

        if (shipment.Condition == Document.Conditions.Unsigned)
        {
            await Document.SignRange([shipment.Guid], data);
        }
        else
        {
            await Document.UnsignRange([shipment.Guid], data);
        }
        return shipment.Guid;
    }
}

