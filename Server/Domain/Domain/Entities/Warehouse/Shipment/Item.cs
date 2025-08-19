namespace Domain.Entities.Warehouse.Shipment;

using Common.Exceptions;
using Domain.Base;
using System.Threading.Tasks;

/// <summary>
/// Ресурс отгрузки
/// </summary>
public sealed class Item : BaseEntity
{
    public interface IRepository : IBaseRepository<Item>
    {
        protected static Item Restore(Guid guid, Guid shipmentGuid, Guid resourceGuid, Guid measureUnitGuid, decimal quantity)
            => new Item(guid, shipmentGuid, resourceGuid, measureUnitGuid, quantity);

        public abstract Task FillByMeasureUnitGuids(List<Guid> unitGuids);
        public abstract Task FillByResourceGuids(List<Guid> resourceGuids);
        public abstract Task FillByShipmentGuids(List<Guid> shipmentGuids);
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
    public static async Task<List<Item>> CreateRange(List<CreateArg> args, IData data)
    {
        var shipmentGuids = args.Select(x => x.ShipmentGuid).Distinct().ToList();
        await data.Shipment.FillByGuids(shipmentGuids);

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

    public record UpdateArg(Guid Guid, Guid ResourceGuid, Guid MeasureUnitGuid, decimal Quantity);
    public static async Task UpdateRange(List<UpdateArg> args, IData data)
    {
        var guids = args.Select(x => x.Guid).Distinct().ToList();
        await data.ShipmentItem.FillByGuids(guids);
        var items = data.ShipmentItem.List.Where(x => guids.Contains(x.Guid)).ToList();

        var shipmentGuids = items.Select(x => x.ShipmentGuid).Distinct().ToList();
        await data.Shipment.FillByGuids(shipmentGuids);

        if (data.Shipment.List.Where(x => shipmentGuids.Contains(x.Guid)).Any(x => x.Condition == Document.Conditions.Signed))
            throw new DomainException("Невозможно изменить ресурс в подписанной отгрузке");

        foreach (var item in items)
        {
            var arg = args.First(x => x.Guid == item.Guid);

            item.ResourceGuid = arg.ResourceGuid;
            item.MeasureUnitGuid = arg.MeasureUnitGuid;
            item.Quantity = arg.Quantity;
            item.Update();
        }
    }

    public static async Task DeleteRange(List<Guid> guids, IData data)
    {
        await data.ShipmentItem.FillByGuids(guids);
        var items = data.ShipmentItem.List.Where(x => guids.Contains(x.Guid)).ToList();

        var shipmentGuids = items.Select(x => x.ShipmentGuid).Distinct().ToList();
        await data.Shipment.FillByGuids(shipmentGuids);

        if (data.Shipment.List.Where(x => shipmentGuids.Contains(x.Guid)).Any(x => x.Condition == Document.Conditions.Signed))
            throw new DomainException("Невозможно удалить ресурс из подписанной отгрузки");

        foreach (var arg in items)
            arg.Remove();
        
        await data.ShipmentItem.FillByShipmentGuids(shipmentGuids);

        foreach (var sg in shipmentGuids)
        {
            if (!data.ShipmentItem.List.Any(x => x.ShipmentGuid == sg))
                throw new DomainException("Невозможно удалить все ресурсы из отгрузки");
        }
    }
}
