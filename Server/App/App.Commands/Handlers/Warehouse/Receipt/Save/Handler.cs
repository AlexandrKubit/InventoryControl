namespace App.Commands.Handlers.Warehouse.Receipt.Save;

using App.Base.Mediator;
using Domain.Base;
using Domain.Entities.Warehouse.Receipt;
using Exchange.Commands.Warehouse.Receipt.Save;
using System.Threading.Tasks;

[RequestRoute("/Warehouse/Receipt/Save", RequestRouteAttribute.Types.Command)]
public class Handler : IRequestHandler<Request, Guid>
{
    public async Task<Guid> HandleAsync(Request request, IServiceProvider provider)
    {
        var data = (IData)provider.GetService(typeof(IData));

        if (request.Guid == Guid.Empty)
        {
            var receipt = (await Document.CreateRange([new Document.CreateArg(request.Number, request.Date.Date)], data)).First();
            var itemArgs = request.Items
                .Select(x => new Item.CreateArg(receipt.Guid, x.ResourceGuid, x.MeasureUnitGuid, x.Quantity))
                .ToList();

            await Item.CreateRange(itemArgs, data);
            return receipt.Guid;
        }
        else
        {
            await Document.UpdateRange([new Document.UpdateArg(request.Guid, request.Number, request.Date)], data);

            await data.ReceiptItem.FillByReceiptGuids([request.Guid]);
            var items = data.ReceiptItem.List.Where(x => x.ReceiptGuid == request.Guid).ToList();

            // delete
            var deletedItemsGuids = items.Select(x => x.Guid).Except(request.Items.Select(x => x.Guid)).ToList();
            await Item.DeleteRange(deletedItemsGuids.ToList(), data);

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

