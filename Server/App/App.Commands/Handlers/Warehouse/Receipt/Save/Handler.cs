namespace App.Commands.Handlers.Warehouse.Receipt.Save;

using App.Base.Mediator;
using Domain.Base;
using Domain.Entities.Warehouse.Receipt;
using Exchange.Commands.Warehouse.Receipt.Save;
using System.Threading.Tasks;

[RequestRoute("/Warehouse/Receipt/Save", RequestRouteAttribute.Types.Command)]
public class Handler(IData data, IUnitOfWork uow) : IRequestHandler<Request, Guid>
{
    // представим что редактирование поступления - это узкое место в нашей системе
    // множество пользователей одновременно редактируют поступлления
    System.Data.IsolationLevel IBaseRequestHandler.IsolationLevel 
        => System.Data.IsolationLevel.ReadCommitted;

    // на самом деле указанных блокировок недостаточно, для гарантирования целостности
    // нужно блокировать и ресурсы и единицы измерения и другие спорные ресурсы
    // они показаны только для примера
    public async Task<Guid> HandleAsync(Request request)
    {
        if (request.Guid == Guid.Empty)
        {
            // При ReadCommitted проверка уникальности номера не защищена
            // Две параллельные транзакции могут одновременно не увидеть номер
            // и создать два документа с одинаковым номером, 
            // что нарушит бизнес-правило.
            await uow.AcquireLock(typeof(Document), request.Number);

            var arg = new Document.CreateArg(request.Number, request.Date.Date);
            var receipt = (await Document.CreateRange([arg], data)).First();
            var itemArgs = request.Items
                .Select(x => new Item.CreateArg(
                    receipt.Guid, 
                    x.ResourceGuid, 
                    x.MeasureUnitGuid, 
                    x.Quantity)
                )
                .ToList();

            await Item.CreateRange(itemArgs, data);
            return receipt.Guid;
        }
        else
        {
            // получаем эксклюзивную блокировку на документ
            // фантомные вставки становятся невозможны, потому что
            // никто не может вставить новую позицию в документ, 
            // не прочитав и не заблокировав его
            await uow.AcquireLock(typeof(Document), request.Guid.ToString());

            // потенциально две параллельные транзакции могут изменить 
            // два различных поступления используя один и тот же номер
            // что приведет к тому что в БД будет два документа с одинаковым номером
            await uow.AcquireLock(typeof(Document), request.Number); 

            var updateArg = new Document.UpdateArg(
                request.Guid, 
                request.Number, 
                request.Date
            );
            await Document.UpdateRange([updateArg], data);

            await data.ReceiptItems.EnsureByReceiptGuids([request.Guid]);
            var items = data.ReceiptItems.List
                .Where(x => x.ReceiptGuid == request.Guid)
                .ToList();

            // delete
            var deletedItemsGuids = items
                .Select(x => x.Guid)
                .Except(request.Items.Select(x => x.Guid))
                .ToList();
            await Item.DeleteRange(deletedItemsGuids.ToHashSet(), data);

            // create
            var createdItems = request.Items.Where(x => x.Guid == Guid.Empty).ToList();
            var createArgs = createdItems
                .Select(x => new Item.CreateArg(
                    request.Guid, 
                    x.ResourceGuid, 
                    x.MeasureUnitGuid, 
                    x.Quantity)
                )
                .ToList();
            await Item.CreateRange(createArgs, data);

            // update
            var updatedItems = request.Items
                .Where(x => x.Guid != Guid.Empty)
                .ToList();
            var updateArgs = updatedItems
                .Select(x => new Item.UpdateArg(
                    x.Guid, 
                    x.ResourceGuid, 
                    x.MeasureUnitGuid, 
                    x.Quantity)
                )
                .ToList();
            await Item.UpdateRange(updateArgs, data);

            return request.Guid;
        }
    }
}

