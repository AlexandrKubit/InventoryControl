namespace Domain.Entities.Warehouse.Receipt;

using Domain.Base;
using Domain.Entities.Warehouse;
using System.Collections.Generic;

/// <summary>
/// Ресурс накладной
/// </summary>
public sealed class Item : BaseEntity
{
    public interface IRepository : IBaseRepository<Item>
    {
        protected static Item Restore(Guid guid, Guid receiptGuid, Guid resourceGuid, Guid measureUnitGuid, decimal quantity)
            => new Item(guid, receiptGuid, resourceGuid, measureUnitGuid, quantity);
    }


    // при создании, изменении и удалении ресурсов накладной, необходимо изменять кол-во ресурсов на складе
    // ресурсы на складе просто так нельзя удалять, добавлять или изменять на складе
    // поэтому эти методы приватны и мы передаем их тем сущностям, которые имеют право изменять кол-во ресурсов на складе
    #region delegates
    private static Action<List<Balance.AddRangeToStockArg>, IData> addRangeToStock;
    private static Action<List<Balance.RemoveRangeFromStockArg>, IData> removeRangeFromStock;
    public static void SetAddRangeToStock(Action<List<Balance.AddRangeToStockArg>, IData> action) 
        => addRangeToStock ??= action;
    public static void SetRemoveRangeFromStock(Action<List<Balance.RemoveRangeFromStockArg>, IData> action) 
        => removeRangeFromStock ??= action;
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
    public static List<Item> CreateRange(List<CreateArg> args, IData data)
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

        addRangeToStock.Invoke(addRangeToStockArgs, data);
        return items;
    }

    public record UpdateArg(Item Item)
    {
        public Guid ResourceGuid = Item.ResourceGuid;
        public Guid MeasureUnitGuid = Item.MeasureUnitGuid;
        public decimal Quantity = Item.Quantity;
    };
    public static void UpdateRange(List<UpdateArg> args, IData data)
    {
        List<Balance.RemoveRangeFromStockArg> removeRangeFromStockArgs = args
            .Where(x => x.Quantity < x.Item.Quantity && x.Item.ResourceGuid == x.ResourceGuid && x.Item.MeasureUnitGuid == x.MeasureUnitGuid)
            .Select(x => new Balance.RemoveRangeFromStockArg(x.Item.ResourceGuid, x.Item.MeasureUnitGuid, x.Item.Quantity - x.Quantity))
            .ToList();
        removeRangeFromStockArgs.AddRange(args
            .Where(x => x.Item.ResourceGuid != x.ResourceGuid || x.Item.MeasureUnitGuid != x.MeasureUnitGuid)
            .Select(x => new Balance.RemoveRangeFromStockArg(x.Item.ResourceGuid, x.Item.MeasureUnitGuid, x.Item.Quantity))
            .ToList()
        );

        List<Balance.AddRangeToStockArg> addRangeToStockArgs = args
            .Where(x => x.Quantity > x.Item.Quantity && x.Item.ResourceGuid == x.ResourceGuid && x.Item.MeasureUnitGuid == x.MeasureUnitGuid)
            .Select(x => new Balance.AddRangeToStockArg(x.Item.ResourceGuid, x.Item.MeasureUnitGuid, x.Quantity - x.Item.Quantity))
            .ToList();
        addRangeToStockArgs.AddRange(args
            .Where(x => x.Item.ResourceGuid != x.ResourceGuid || x.Item.MeasureUnitGuid != x.MeasureUnitGuid)
            .Select(x => new Balance.AddRangeToStockArg(x.Item.ResourceGuid, x.Item.MeasureUnitGuid, x.Item.Quantity))
            .ToList()
        );

        foreach (var arg in args)
        {
            arg.Item.ResourceGuid = arg.ResourceGuid;
            arg.Item.MeasureUnitGuid = arg.MeasureUnitGuid;
            arg.Item.Quantity = arg.Quantity;
            arg.Item.Update();
        }

        removeRangeFromStock.Invoke(removeRangeFromStockArgs, data);
        addRangeToStock.Invoke(addRangeToStockArgs, data);
    }

    public static void DeleteRange(List<Item> args, IData data)
    {
        List<Balance.RemoveRangeFromStockArg> removeRangeFromStockArgs = args
            .Select(x => new Balance.RemoveRangeFromStockArg(x.ResourceGuid, x.MeasureUnitGuid, x.Quantity))
            .ToList();

        removeRangeFromStock.Invoke(removeRangeFromStockArgs, data);

        foreach (var arg in args)
            arg.Remove();
    }
}
