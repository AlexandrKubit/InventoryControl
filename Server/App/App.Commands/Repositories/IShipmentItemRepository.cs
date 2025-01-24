namespace App.Commands.Repositories;

using App.Commands.Base;
using Domain.Entities.Warehouse.Shipment;

public interface IShipmentItemRepository : Item.IRepository, IRepository
{
    public abstract Task FillByMeasureUnitGuids(List<Guid> unitGuids);
    public abstract Task FillByResourceGuids(List<Guid> resourceGuids);
    public abstract Task FillByShipmentGuids(List<Guid> shipmentGuids);
}
