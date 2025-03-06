namespace App.Commands.Handlers.Warehouse.Receipt.Save;

using App.Base.Mediator;
using App.Commands.Base;
using Domain.Entities.Warehouse.Receipt;
using Exchange.Commands.Warehouse.Receipt.Save;
using System.Threading.Tasks;

[RequestRoute("/Warehouse/Receipt/Save", RequestRouteAttribute.Types.Command)]
public class Handler : IRequestHandler<Request, Guid>
{
    public async Task<Guid> HandleAsync(Request request, IServiceProvider provider)
    {
        var uow = (IUnitOfWork)provider.GetService(typeof(IUnitOfWork));

        await uow.Receipt.FillByNumbers([request.Number]);

        if (request.Guid == Guid.Empty)
        {
            var receipt = Document.CreateRange([new Document.CreateArg(request.Number, request.Date.Date)], uow).First();
            var itemArgs = request.Items
                .Select(x => new Item.CreateArg(receipt.Guid, x.ResourceGuid, x.MeasureUnitGuid, x.Quantity))
                .ToList();

            var args = request.Items.Select(x => (x.ResourceGuid, x.MeasureUnitGuid));
            await uow.Balance.FillByResourceMeasureUnit(args);

            Item.CreateRange(itemArgs, uow);
            return receipt.Guid;
        }
        else
        {
            await uow.Receipt.FillByGuids([request.Guid]);
            var receipt = uow.Receipt.List.FirstOrDefault(x => x.Guid == request.Guid);

            Document.UpdateRange([new Document.UpdateArg(receipt) with {
                Number = request.Number,
                Date = request.Date.Date
            }], uow);

            await uow.ReceiptItem.FillByReceiptGuids([receipt.Guid]);
            var items = uow.ReceiptItem.List.Where(x => x.ReceiptGuid == receipt.Guid).ToList();

            // items
            var deletedItemsGuids = items.Select(x => x.Guid).Except(request.Items.Select(x => x.Guid)).ToList();
            var deletedItems = items.Where(x => deletedItemsGuids.Contains(x.Guid)).ToList();
            var createdItems = request.Items.Where(x => x.Guid == Guid.Empty).ToList();
            var updatedItems = request.Items.Where(x => x.Guid != Guid.Empty).ToList();

            var args = deletedItems.Select(x => (x.ResourceGuid, x.MeasureUnitGuid)).ToList();
            args.AddRange(createdItems.Select(x => (x.ResourceGuid, x.MeasureUnitGuid)));
            args.AddRange(updatedItems.Select(x => (x.ResourceGuid, x.MeasureUnitGuid)));
            args.AddRange(items.Select(x => (x.ResourceGuid, x.MeasureUnitGuid)));
            args = args.Distinct().ToList();
            await uow.Balance.FillByResourceMeasureUnit(args);

            // delete
            Item.DeleteRange(deletedItems, uow);

            // create
            var createArgs = createdItems
                .Select(x => new Item.CreateArg(receipt.Guid, x.ResourceGuid, x.MeasureUnitGuid, x.Quantity))
                .ToList();

            Item.CreateRange(createArgs, uow);

            // update
            var updateArgs = updatedItems
                .Select(x => new Item.UpdateArg(items.First(y => y.Guid == x.Guid)) with
                {
                    ResourceGuid = x.ResourceGuid,
                    MeasureUnitGuid = x.MeasureUnitGuid,
                    Quantity = x.Quantity
                }).ToList();

            Item.UpdateRange(updateArgs, uow);

            return receipt.Guid;
        }
    }
}

