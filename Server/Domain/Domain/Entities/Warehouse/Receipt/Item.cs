namespace Domain.Entities.Warehouse.Receipt;

using Domain.Base;
using Domain.Entities.Warehouse;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Ресурс накладной
/// </summary>
public sealed class Item : BaseEntity
{
    public interface IRepository : IBaseRepository<Item>
    {
        protected static Item Restore(Guid guid, Guid receiptGuid, Guid resourceGuid, Guid measureUnitGuid, decimal quantity)
            => new Item(guid, receiptGuid, resourceGuid, measureUnitGuid, quantity);

        public abstract Task FillByMeasureUnitGuids(List<Guid> unitGuids);
        public abstract Task FillByResourceGuids(List<Guid> resourceGuids);
        public abstract Task FillByReceiptGuids(List<Guid> receiptGuids);
    }


    // при создании, изменении и удалении ресурсов накладной, необходимо изменять кол-во ресурсов на складе
    // ресурсы на складе просто так нельзя удалять, добавлять или изменять на складе
    // поэтому эти методы приватны и мы передаем их тем сущностям, которые имеют право изменять кол-во ресурсов на складе
    #region delegates
    private static Func<List<Balance.AddRangeToStockArg>, IData, Task> addRangeToStock;
    private static Func<List<Balance.RemoveRangeFromStockArg>, IData, Task> removeRangeFromStock;
    public static void SetAddRangeToStock(Func<List<Balance.AddRangeToStockArg>, IData, Task> func) => addRangeToStock ??= func;
    public static void SetRemoveRangeFromStock(Func<List<Balance.RemoveRangeFromStockArg>, IData, Task> func) => removeRangeFromStock ??= func;
    #endregion

    public Guid ReceiptGuid { get; }
    public Guid ResourceGuid { get; private set; }
    public Guid MeasureUnitGuid { get; private set; }
    public decimal Quantity { get; private set; }

    private Item(Guid guid, Guid receiptGuid, Guid resourceGuid, Guid measureUnitGuid, decimal quantity)
    {
        Guid = guid;
        ReceiptGuid = receiptGuid;
        ResourceGuid = resourceGuid;
        MeasureUnitGuid = measureUnitGuid;
        Quantity = quantity;
    }

    public record CreateArg(Guid ReceiptGuid, Guid ResourceGuid, Guid MeasureUnitGuid, decimal Quantity);
    public static async Task<List<Item>> CreateRange(List<CreateArg> args, IData data)
    {
        List<Item> items = new List<Item>();

        foreach (var arg in args)
        {
            var item = new Item(Guid.CreateVersion7(), arg.ReceiptGuid, arg.ResourceGuid, arg.MeasureUnitGuid, arg.Quantity);
            item.Append(data.ReceiptItem);
            items.Add(item);
        }

        List<Balance.AddRangeToStockArg> addRangeToStockArgs = items
            .Select(x => new Balance.AddRangeToStockArg(x.ResourceGuid, x.MeasureUnitGuid, x.Quantity))
            .ToList();

        await addRangeToStock(addRangeToStockArgs, data);
        return items;
    }

    public record UpdateArg(Guid Guid, Guid ResourceGuid, Guid MeasureUnitGuid, decimal Quantity);
    public static async Task UpdateRange(List<UpdateArg> args, IData data)
    {
        var guids = args.Select(x => x.Guid).Distinct().ToList();
        await data.ReceiptItem.FillByGuids(guids);
        var items = data.ReceiptItem.List.Where(x => guids.Contains(x.Guid)).ToList();

        var removeRangeFromStockArgs = items
            .Select(x => new Balance.RemoveRangeFromStockArg(x.ResourceGuid, x.MeasureUnitGuid, x.Quantity))
            .ToList();

        await removeRangeFromStock(removeRangeFromStockArgs, data);

        foreach (var item in items)
        {
            var arg = args.First(x => x.Guid == item.Guid);

            item.ResourceGuid = arg.ResourceGuid;
            item.MeasureUnitGuid = arg.MeasureUnitGuid;
            item.Quantity = arg.Quantity;
            item.Update();
        }

        var addRangeToStockArgs = items
            .Select(x => new Balance.AddRangeToStockArg(x.ResourceGuid, x.MeasureUnitGuid, x.Quantity))
            .ToList();

        await addRangeToStock(addRangeToStockArgs, data);
    }


    public static async Task DeleteRange(List<Guid> guids, IData data)
    {
        await data.ReceiptItem.FillByGuids(guids);
        var items = data.ReceiptItem.List.Where(x => guids.Contains(x.Guid)).ToList();

        var removeRangeFromStockArgs = items
            .Select(x => new Balance.RemoveRangeFromStockArg(x.ResourceGuid, x.MeasureUnitGuid, x.Quantity))
            .ToList();

        await removeRangeFromStock(removeRangeFromStockArgs, data);

        foreach (var item in items)
            item.Remove();
    }
}
