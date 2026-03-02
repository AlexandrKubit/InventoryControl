namespace Domain.Entities.Warehouse.Receipt;

using Domain.Base;
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


    // при создании, изменении и удалении ресурсов поступления, необходимо изменять кол-во ресурсов на складе (баланс)
    // ресурсы на складе просто так нельзя удалять, добавлять или изменять на складе
    // поэтому баланс подписывается на эти события
    #region Events
    public record CreatedRangeArg(List<Item> Items, IData Data);
    public static Action<Func<CreatedRangeArg, Task>> OnCreatedRange => CreatedRange.Subscribe;
    private static readonly DomainEvent<CreatedRangeArg> CreatedRange = new(); // один подписчик - баланс на складе, порядок не важен  


    // представим что у нас было бы несколько подписчиков и был бы важен порядок
    private static readonly Type[] UpdatedRangeOrder = [
        typeof(Balance), // сначала пересчитать баланс при изменении поступления
        //typeof(SomeEntity), // потом сделать что то
    ];
    public record ItemData(Guid ResourceGuid, Guid MeasureUnitGuid, decimal Quantity);
    public record UpdatedRangeArg(List<(ItemData Old, ItemData New)> Changes, IData Data);
    public static Action<Func<UpdatedRangeArg, Task>> OnUpdatedRange => UpdatedRange.Subscribe;
    private static readonly DomainEvent<UpdatedRangeArg> UpdatedRange = new(UpdatedRangeOrder); // порядок важен

    public record DeletedRangeArg(List<Item> Items, IData Data);
    public static Action<Func<DeletedRangeArg, Task>> OnDeletedRange => DeletedRange.Subscribe;
    private static readonly DomainEvent<DeletedRangeArg> DeletedRange = new(); // один подписчик - баланс на складе, порядок не важен  
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

        await CreatedRange.Invoke(new CreatedRangeArg(items, data));
        return items;
    }

    public record UpdateArg(Guid Guid, Guid ResourceGuid, Guid MeasureUnitGuid, decimal Quantity);
    public static async Task UpdateRange(List<UpdateArg> args, IData data)
    {
        var guids = args.Select(x => x.Guid).Distinct().ToList();
        await data.ReceiptItem.FillByGuids(guids);
        var items = data.ReceiptItem.List.Where(x => guids.Contains(x.Guid)).ToList();

        List<(ItemData Old, ItemData New)> сhanges = [];

        foreach (var item in items)
        {
            var arg = args.First(x => x.Guid == item.Guid);

            ItemData old = new(item.ResourceGuid, item.MeasureUnitGuid, item.Quantity);
            ItemData _new = new(arg.ResourceGuid, arg.MeasureUnitGuid, arg.Quantity);
            сhanges.Add((old, _new));

            item.ResourceGuid = arg.ResourceGuid;
            item.MeasureUnitGuid = arg.MeasureUnitGuid;
            item.Quantity = arg.Quantity;
            item.Update();
        }

        await UpdatedRange.Invoke(new UpdatedRangeArg(сhanges, data));
    }


    public static async Task DeleteRange(List<Guid> guids, IData data)
    {
        await data.ReceiptItem.FillByGuids(guids);
        var items = data.ReceiptItem.List.Where(x => guids.Contains(x.Guid)).ToList();

        foreach (var item in items)
            item.Remove();

        await DeletedRange.Invoke(new DeletedRangeArg(items, data));
    }
}