namespace App.Commands.Handlers.Warehouse.Shipment.Save;

using App.Base.Mediator;
using Domain.Base;
using Domain.Entities.Warehouse.Shipment;
using Exchange.Commands.Warehouse.Shipment.Save;
using System.Threading.Tasks;

[RequestRoute("/Warehouse/Shipment/Save", RequestRouteAttribute.Types.Command)]
public class Handler : IRequestHandler<Request, Guid>
{
    public async Task<Guid> HandleAsync(Request request, IServiceProvider provider)
    {
        var data = (IData)provider.GetService(typeof(IData));

        if (request.Guid == Guid.Empty)
        {
            var itemArgs = request.Items
                .Select(x => new Item.CreateArg(Guid.Empty, x.ResourceGuid, x.MeasureUnitGuid, x.Quantity))
                .ToList();
            var arg = new Document.CreateArg(request.Number, request.ClientGuid, request.Date, itemArgs);
            var shipment = (await Document.CreateRange([arg], data)).First();
            return shipment.Guid;
        }
        else
        {
            await Document.UpdateRange([new Document.UpdateArg(request.Guid, request.Number, request.ClientGuid, request.Date)], data);

            await data.ShipmentItem.FillByShipmentGuids([request.Guid]);
            var items = data.ShipmentItem.List.Where(x => x.ShipmentGuid == request.Guid).ToList();

            // delete
            var deletedItemsGuids = items.Select(x => x.Guid).Except(request.Items.Select(x => x.Guid)).ToList();
            await Item.DeleteRange(deletedItemsGuids, data);

            // create
            var createdItems = request.Items.Where(x => x.Guid == Guid.Empty).ToList();
            var createArgs = createdItems.Select(x => new Item.CreateArg(request.Guid, x.ResourceGuid, x.MeasureUnitGuid, x.Quantity)).ToList();
            await Item.CreateRange(createArgs, data);

            // update
            var updatedItems = request.Items.Where(x => x.Guid != Guid.Empty).ToList();
            var updateArgs = updatedItems.Select(x => new Item.UpdateArg(x.Guid, x.ResourceGuid, x.MeasureUnitGuid, x.Quantity)).ToList();
            await Item.UpdateRange(updateArgs, data);

            return request.Guid;
        }
    }
}

