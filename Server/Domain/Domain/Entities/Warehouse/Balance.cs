namespace Domain.Entities.Warehouse;

using Common.Exceptions;
using Domain.Base;
using System;
using System.Threading.Tasks;

/// <summary>
/// Баланс (свободный остаток на склада)
/// </summary>
public sealed class Balance : BaseEntity
{
    public interface IRepository : IBaseRepository<Balance>
    {
        protected static Balance Restore(Guid guid, Guid resourceGuid, Guid measureUnitGuid, decimal quantity)
            => new Balance(guid, resourceGuid, measureUnitGuid, quantity);

        public Task FillByResourceMeasureUnit(IEnumerable<(Guid ResourceGuid, Guid MeasureUnitGuid)> args);
        public Task FillByMeasureUnitGuids(List<Guid> unitGuids);
        public Task FillByResourceGuids(List<Guid> resourceGuids);
    }

    // подписываемся на события 
    static Balance()
    {
        Receipt.Item.OnCreatedRange(OnReceiptItemCreatedRangeHandler);
        Receipt.Item.OnUpdatedRange(OnReceiptItemUpdatedRangeHandler);
        Receipt.Item.OnDeletedRange(OnReceiptItemDeletedRangeHandler);

        Shipment.Document.OnSignedRange(OnShipmentDocumentSignedRangeHandler);
        Shipment.Document.OnUnsignedRange(OnShipmentDocumentUnsignedRangeHandler);
    }

    public Guid ResourceGuid { get; }
    public Guid MeasureUnitGuid { get; }
    public decimal Quantity { get; private set; }

    private Balance(Guid guid, Guid resourceGuid, Guid measureUnitGuid, decimal quantity)
    {
        Guid = guid;
        ResourceGuid = resourceGuid;
        MeasureUnitGuid = measureUnitGuid;
        Quantity = quantity;
    }

    public record AddRangeToStockArg(Guid ResourceGuid, Guid MeasureUnitGuid, decimal Quantity);
    private static async Task AddRangeToStock(List<AddRangeToStockArg> args, IData data)
    {
        var resourceMeasureUnits = args.Select(x => (x.ResourceGuid, x.MeasureUnitGuid)).ToList();
        await data.Balance.FillByResourceMeasureUnit(resourceMeasureUnits);

        foreach (var arg in args)
        {
            var balance = data.Balance.List.FirstOrDefault(x => x.MeasureUnitGuid == arg.MeasureUnitGuid && x.ResourceGuid == arg.ResourceGuid);
            if (balance != null)
            {
                balance.Quantity += arg.Quantity;
                balance.Update();
            }
            else
            {
                balance = new Balance(Guid.CreateVersion7(), arg.ResourceGuid, arg.MeasureUnitGuid, arg.Quantity);
                balance.Append(data.Balance);
            }
        }
    }

    public record RemoveRangeFromStockArg(Guid ResourceGuid, Guid MeasureUnitGuid, decimal Quantity);
    private static async Task RemoveRangeFromStock(List<RemoveRangeFromStockArg> args, IData data)
    {
        var resourceMeasureUnits = args.Select(x => (x.ResourceGuid, x.MeasureUnitGuid)).ToList();
        await data.Balance.FillByResourceMeasureUnit(resourceMeasureUnits);

        foreach (var arg in args)
        {
            var balance = data.Balance.List.FirstOrDefault(x => x.MeasureUnitGuid == arg.MeasureUnitGuid && x.ResourceGuid == arg.ResourceGuid);
            if (balance != null)
            {
                if (balance.Quantity < arg.Quantity)
                    throw new DomainException("На складе не достаточно ресурсов");

                balance.Quantity -= arg.Quantity;
                if (balance.Quantity > 0)
                    balance.Update();
                else
                    balance.Remove();
            }
            else
            {
                throw new DomainException("На складе отсутствуют ресурсы");
            }
        }
    }


    private static async Task OnReceiptItemCreatedRangeHandler(Receipt.Item.CreatedRangeArg arg)
    {
        var args = arg.Items.Select(x => new AddRangeToStockArg(x.ResourceGuid, x.MeasureUnitGuid, x.Quantity)).ToList();
        await AddRangeToStock(args, arg.Data);
    }

    private static async Task OnReceiptItemUpdatedRangeHandler(Receipt.Item.UpdatedRangeArg arg)
    {
        var netChanges = new Dictionary<(Guid ResourceGuid, Guid MeasureUnitGuid), decimal>();

        foreach (var change in arg.Changes)
        {
            var oldKey = (change.Old.ResourceGuid, change.Old.MeasureUnitGuid);
            var newKey = (change.New.ResourceGuid, change.New.MeasureUnitGuid);

            // Вычитаем старое количество
            if (netChanges.TryGetValue(oldKey, out var oldDelta))
                netChanges[oldKey] = oldDelta - change.Old.Quantity;
            else
                netChanges[oldKey] = -change.Old.Quantity;

            // Добавляем новое количество
            if (netChanges.TryGetValue(newKey, out var newDelta))
                netChanges[newKey] = newDelta + change.New.Quantity;
            else
                netChanges[newKey] = change.New.Quantity;
        }

        var toRemove = new List<Balance.RemoveRangeFromStockArg>();
        var toAdd = new List<Balance.AddRangeToStockArg>();

        foreach (var kv in netChanges)
        {
            if (kv.Value < 0)
            {
                toRemove.Add(new Balance.RemoveRangeFromStockArg(
                    kv.Key.ResourceGuid,
                    kv.Key.MeasureUnitGuid,
                    -kv.Value)); // передаём положительное количество для списания
            }
            else if (kv.Value > 0)
            {
                toAdd.Add(new Balance.AddRangeToStockArg(
                    kv.Key.ResourceGuid,
                    kv.Key.MeasureUnitGuid,
                    kv.Value));
            }
            // При kv.Value == 0 изменений нет, пропускаем
        }

        // Применяем изменения в правильном порядке (сначала списание, потом добавление)
        if (toRemove.Any())
            await RemoveRangeFromStock(toRemove, arg.Data);
        if (toAdd.Any())
            await AddRangeToStock(toAdd, arg.Data);
    }

    private static async Task OnReceiptItemDeletedRangeHandler(Receipt.Item.DeletedRangeArg arg)
    {
        var args = arg.Items.Select(x => new RemoveRangeFromStockArg(x.ResourceGuid, x.MeasureUnitGuid, x.Quantity)).ToList();
        await RemoveRangeFromStock(args, arg.Data);
    }

    private static async Task OnShipmentDocumentSignedRangeHandler(Shipment.Document.SignedRangeArg arg)
    {
        var guids = arg.Documents.Select(x => x.Guid).Distinct().ToList();

        await arg.Data.ShipmentItem.FillByShipmentGuids(guids);
        var items = arg.Data.ShipmentItem.List.Where(x => guids.Contains(x.ShipmentGuid)).ToList();

        var removeRangeFromStockArgs = items
            .Select(x => new RemoveRangeFromStockArg(x.ResourceGuid, x.MeasureUnitGuid, x.Quantity))
            .ToList();

        await RemoveRangeFromStock(removeRangeFromStockArgs, arg.Data);
    }

    private static async Task OnShipmentDocumentUnsignedRangeHandler(Shipment.Document.UnsignedRangeArg arg)
    {
        var guids = arg.Documents.Select(x => x.Guid).Distinct().ToList();

        await arg.Data.ShipmentItem.FillByShipmentGuids(guids);
        var items = arg.Data.ShipmentItem.List.Where(x => guids.Contains(x.ShipmentGuid)).ToList();

        var addRangeToStockArgs = items
            .Select(x => new AddRangeToStockArg(x.ResourceGuid, x.MeasureUnitGuid, x.Quantity))
            .ToList();

        await AddRangeToStock(addRangeToStockArgs, arg.Data);
    }
}
