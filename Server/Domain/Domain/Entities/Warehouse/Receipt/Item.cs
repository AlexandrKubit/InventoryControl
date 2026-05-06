namespace Domain.Entities.Warehouse.Receipt;

using Common.Exceptions;
using Domain.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Ресурс накладной
/// </summary>
public sealed class Item : BaseEntity
{
    static Item()
    {
        Directories.MeasureUnit.OnDeletedRange(OnMeasureUnitDeletedRangeHandler);
    }
    
    public interface IRepository : IBaseRepository<Item>
    {
        protected static Item Restore(Guid guid, Guid receiptGuid, Guid resourceGuid, Guid measureUnitGuid, decimal quantity)
            => new Item(guid, receiptGuid, resourceGuid, measureUnitGuid, quantity);

        public abstract Task EnsureByMeasureUnitGuids(HashSet<Guid> unitGuids);
        public abstract Task EnsureByResourceGuids(HashSet<Guid> resourceGuids);
        public abstract Task EnsureByReceiptGuids(HashSet<Guid> receiptGuids);
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

        var resourceGuids = args.Select(x => x.ResourceGuid).ToHashSet();
        var unitGuids = args.Select(x => x.MeasureUnitGuid).ToHashSet();

        await data.Resources.EnsureByGuids(resourceGuids);
        await data.MeasureUnits.EnsureByGuids(unitGuids);

        foreach (var arg in args)
        {
            var resource = data.Resources.List.FirstOrDefault(x => x.Guid == arg.ResourceGuid);
            if(resource == null || resource.Condition == Directories.Resource.Conditions.Archive)
                throw new DomainException("Ресурс удален или переведен в архив");

            var unit = data.MeasureUnits.List.FirstOrDefault(x => x.Guid == arg.MeasureUnitGuid);
            if (unit == null || unit.Condition == Directories.MeasureUnit.Conditions.Archive)
                throw new DomainException("Единица измерения удалена или переведена в архив");

            var item = new Item(Guid.CreateVersion7(), arg.ReceiptGuid, arg.ResourceGuid, arg.MeasureUnitGuid, arg.Quantity);
            item.Create();
			data.ReceiptItems.Add(item);
			items.Add(item);
        }

        await CreatedRange.Invoke(new CreatedRangeArg(items, data));
        return items;
    }

    public record UpdateArg(Guid Guid, Guid ResourceGuid, Guid MeasureUnitGuid, decimal Quantity);
    public static async Task UpdateRange(List<UpdateArg> args, IData data)
    {
        var guids = args.Select(x => x.Guid).ToHashSet();
        await data.ReceiptItems.EnsureByGuids(guids);
        var items = data.ReceiptItems.List.Where(x => guids.Contains(x.Guid)).ToList();

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


    public static async Task DeleteRange(HashSet<Guid> guids, IData data)
    {
        await data.ReceiptItems.EnsureByGuids(guids);
        var items = data.ReceiptItems.List.Where(x => guids.Contains(x.Guid)).ToList();

        foreach (var item in items)
            item.Remove();

        await DeletedRange.Invoke(new DeletedRangeArg(items, data));
    }

    private static async Task OnMeasureUnitDeletedRangeHandler(Directories.MeasureUnit.DeletedRangeArg arg)
    {
        await arg.Data.ReceiptItems.EnsureByMeasureUnitGuids(arg.Guids);

        if (arg.Data.ReceiptItems.List.Any(x => arg.Guids.Contains(x.MeasureUnitGuid)))
            throw new DomainException("Невозможно удалить единицу измерения т.к. она используется в поступлениях");
    }
}