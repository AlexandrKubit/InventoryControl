namespace App.Commands.Handlers.Warehouse.Shipment.ChangeCondition;

using App.Base.Mediator;
using App.Commands.Base;
using Domain.Entities.Warehouse.Shipment;
using Exchange.Commands.Warehouse.Shipment.ChangeCondition;
using System.Threading.Tasks;

[RequestRoute("/Warehouse/Shipment/ChangeCondition", RequestRouteAttribute.Types.Command)]
public class Handler : IRequestHandler<Request, Guid>
{
    public async Task<Guid> HandleAsync(Request request, IServiceProvider provider)
    {
        var uow = (IUnitOfWork)provider.GetService(typeof(IUnitOfWork));
        await uow.Shipment.FillByGuids([request.Guid]);
        await uow.ShipmentItem.FillByShipmentGuids([request.Guid]);

        var args = uow.ShipmentItem.List.Where(x => x.ShipmentGuid == request.Guid).Select(x => (x.ResourceGuid, x.MeasureUnitGuid));
        await uow.Balance.FillByResourceMeasureUnit(args);

        var shipment = uow.Shipment.List.FirstOrDefault(x => x.Guid == request.Guid);

        if (shipment.Condition == Document.Conditions.Unsigned)
        {
            Document.SignRange([shipment], uow);
        }
        else
        {
            Document.UnsignRange([shipment], uow);
        }
        return shipment.Guid;
    }
}

