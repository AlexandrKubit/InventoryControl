namespace Domain.Entities.Warehouse.Shipment;

using Common.Exceptions;
using Domain.Base;

/// <summary>
/// Ресурс отгрузки
/// </summary>
public sealed class Item : BaseEntity
{
    public interface IRepository : IBaseRepository<Item>
    {
        protected static Item Restore(Guid guid, Guid shipmentGuid, Guid resourceGuid, Guid measureUnitGuid, decimal quantity)
            => new Item(guid, shipmentGuid, resourceGuid, measureUnitGuid, quantity);

    }

    public Guid ShipmentGuid { get; }
    public Guid ResourceGuid { get; private set; }
    public Guid MeasureUnitGuid { get; private set; }
    public decimal Quantity { get; private set; }

    private Item(Guid guid, Guid shipmentGuid, Guid resourceGuid, Guid measureUnitGuid, decimal quantity)
    {
        Guid = guid;
        ShipmentGuid = shipmentGuid;
        ResourceGuid = resourceGuid;
        MeasureUnitGuid = measureUnitGuid;
        Quantity = quantity;
    }

    public record CreateArg(Guid ShipmentGuid, Guid ResourceGuid, Guid MeasureUnitGuid, decimal Quantity);
    public static List<Item> CreateRange(List<CreateArg> args, IData data)
    {
        var shipmentGuids = args.Select(x => x.ShipmentGuid).ToList();
        if (data.Shipment.List.Where(x => shipmentGuids.Contains(x.Guid)).Any(x => x.Condition == Document.Conditions.Signed))
            throw new DomainException("Невозможно добавить ресурс в подписанную отгрузку");

        List<Item> items = new List<Item>();

        foreach (var arg in args)
        {
            var item = new Item(Guid.CreateVersion7(), arg.ShipmentGuid, arg.ResourceGuid, arg.MeasureUnitGuid, arg.Quantity);
            item.Append(data.ShipmentItem);
            items.Add(item);
        }

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
        var shipmentGuids = args.Select(x => x.Item.ShipmentGuid).ToList();
        if (data.Shipment.List.Where(x => shipmentGuids.Contains(x.Guid)).Any(x => x.Condition == Document.Conditions.Signed))
            throw new DomainException("Невозможно изменить ресурс в подписанной отгрузке");

        foreach (var arg in args)
        {
            arg.Item.ResourceGuid = arg.ResourceGuid;
            arg.Item.MeasureUnitGuid = arg.MeasureUnitGuid;
            arg.Item.Quantity = arg.Quantity;
            arg.Item.Update();
        }
    }

    public static void DeleteRange(List<Item> items, IData data)
    {
        var shipmentGuids = items.Select(x => x.ShipmentGuid).ToList();
        if (data.Shipment.List.Where(x => shipmentGuids.Contains(x.Guid)).Any(x => x.Condition == Document.Conditions.Signed))
            throw new DomainException("Невозможно удалить ресурс из подписанной отгрузки");

        foreach (var arg in items)
            arg.Remove();
    }
}
