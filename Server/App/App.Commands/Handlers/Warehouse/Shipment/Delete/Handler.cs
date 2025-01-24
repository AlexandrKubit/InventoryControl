namespace App.Commands.Handlers.Warehouse.Shipment.Delete;

using App.Base.Mediator;
using App.Commands.Base;
using Domain.Entities.Warehouse.Shipment;
using Exchange.Commands.Warehouse.Shipment.Delete;
using System.Threading.Tasks;

[RequestRoute("/Warehouse/Shipment/Delete", RequestRouteAttribute.Types.Command)]
public class Handler : IRequestHandler<Request, Guid>
{
    public async Task<Guid> HandleAsync(Request request, IServiceProvider provider)
    {
        var uow = (IUnitOfWork)provider.GetService(typeof(IUnitOfWork));

        await uow.Shipment.FillByGuids([request.Guid]);
        await uow.ShipmentItem.FillByShipmentGuids([request.Guid]);

        var shipment = uow.Shipment.List.FirstOrDefault(x => x.Guid == request.Guid);

        Document.DeleteRange([shipment], uow);
        return shipment.Guid;
    }
}

