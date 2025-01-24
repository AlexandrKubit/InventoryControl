namespace App.Commands.Handlers.Warehouse.Shipment.Save;

using App.Base.Mediator;
using App.Commands.Base;
using Domain.Entities.Warehouse.Shipment;
using Exchange.Commands.Warehouse.Shipment.Save;
using System.Threading.Tasks;

[RequestRoute("/Warehouse/Shipment/Save", RequestRouteAttribute.Types.Command)]
public class Handler : IRequestHandler<Request, Guid>
{
    public async Task<Guid> HandleAsync(Request request, IServiceProvider provider)
    {
        var uow = (IUnitOfWork)provider.GetService(typeof(IUnitOfWork));
        await uow.Shipment.FillByNumbers([request.Number]);

        if (request.Guid == Guid.Empty)
        {
            var itemArgs = request.Items
                .Select(x => new Item.CreateArg(Guid.Empty, x.ResourceGuid, x.MeasureUnitGuid, x.Quantity))
                .ToList();
            var shipment = Document.CreateRange(
                [new Document.CreateArg(request.Number, request.ClientGuid, request.Date, itemArgs)], uow
            ).First();

            return shipment.Guid;
        }
        else
        {
            await uow.Shipment.FillByGuids([request.Guid]);
            var shipment = uow.Shipment.List.FirstOrDefault(x => x.Guid == request.Guid);

            Document.UpdateRange([new Document.UpdateArg(shipment) with {
                Number = request.Number,
                ClientGuid = request.ClientGuid,
                Date = request.Date
            }], uow);

            await uow.ShipmentItem.FillByShipmentGuids([shipment.Guid]);
            var items = uow.ShipmentItem.List.Where(x => x.ShipmentGuid == shipment.Guid).ToList();

            // delete
            var deletedItemsGuids = items.Select(x => x.Guid).Except(request.Items.Select(x => x.Guid)).ToList();
            var deletedItems = items.Where(x => deletedItemsGuids.Contains(x.Guid)).ToList();
            Item.DeleteRange(deletedItems, uow);

            // create
            var createdItems = request.Items.Where(x => x.Guid == Guid.Empty).ToList();
            var createArgs = createdItems
                .Select(x => new Item.CreateArg(shipment.Guid, x.ResourceGuid, x.MeasureUnitGuid, x.Quantity))
                .ToList();

            Item.CreateRange(createArgs, uow);

            // update
            var updatedItems = request.Items.Where(x => x.Guid != Guid.Empty).ToList();
            var updateArgs = updatedItems
                .Select(x => new Item.UpdateArg(items.First(y => y.Guid == x.Guid)) with
                {
                    ResourceGuid = x.ResourceGuid,
                    MeasureUnitGuid = x.MeasureUnitGuid,
                    Quantity = x.Quantity
                }).ToList();

            Item.UpdateRange(updateArgs, uow);

            return shipment.Guid;
        }
    }
}

