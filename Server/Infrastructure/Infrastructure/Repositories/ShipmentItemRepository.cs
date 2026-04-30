namespace Infrastructure.Repositories;

using Domain.Entities.Warehouse.Shipment;
using Infrastructure.Base;
using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;

internal class ShipmentItemRepository : BaseRepository<Item>, Item.IRepository
{
    public ShipmentItemRepository(UnitOfWork uow)
    {
        this.context = uow.Context;
    }

    private Context context { get; set; }

    private Item Restore(Entities.ShipmentItem item) =>
        Item.IRepository.Restore(item.Guid, item.ShipmentGuid, item.ResourceGuid, item.MeasureUnitGuid, item.Quantity);

    public async Task EnsureByMeasureUnitGuids(HashSet<Guid> unitGuids)
    {
        var func = async (IEnumerable<Guid> args) =>
            await context.ShipmentItems
                .Where(x => args.Contains(x.MeasureUnitGuid))
                .Where(x => !LoadedGuids.Contains(x.Guid))
                .ToDictionaryAsync(x => x.Guid, x => Restore(x));

        await LoadWithCacheAsync(unitGuids, func);
    }

    public async Task EnsureByShipmentGuids(HashSet<Guid> shipmentGuids)
    {
        var func = async (IEnumerable<Guid> args) =>
            await context.ShipmentItems
                .Where(x => args.Contains(x.ShipmentGuid))
                .Where(x => !LoadedGuids.Contains(x.Guid))
                .ToDictionaryAsync(x => x.Guid, x => Restore(x));

        await LoadWithCacheAsync(shipmentGuids, func);
    }

    public async Task EnsureByResourceGuids(HashSet<Guid> resourceGuids)
    {
        var func = async (IEnumerable<Guid> args) =>
            await context.ShipmentItems
                .Where(x => args.Contains(x.ResourceGuid))
                .Where(x => !LoadedGuids.Contains(x.Guid))
                .ToDictionaryAsync(x => x.Guid, x => Restore(x));

        await LoadWithCacheAsync(resourceGuids, func);
    }

    public override void Commit()
    {
        EntityCommitHelper.CommitEntities(
            dbSet: context.ShipmentItems,
            entities: collection.Values,
            createMapDelegate: entity => new Entities.ShipmentItem
            {
                Guid = entity.Guid,
                ShipmentGuid = entity.ShipmentGuid,
                ResourceGuid = entity.ResourceGuid,
                MeasureUnitGuid = entity.MeasureUnitGuid,
                Quantity = entity.Quantity
            },
            updateMapDelegate: (dbEntity, entity) =>
            {
                dbEntity.ShipmentGuid = entity.ShipmentGuid;
                dbEntity.ResourceGuid = entity.ResourceGuid;
                dbEntity.MeasureUnitGuid = entity.MeasureUnitGuid;
                dbEntity.Quantity = entity.Quantity;
            }
        );
    }

    protected override async Task<Dictionary<Guid, Item>> GetFromDbByGuidsAsync(HashSet<Guid> guids)
    {
        return await context.ShipmentItems
            .Where(x => guids.Contains(x.Guid))
            .ToDictionaryAsync(x => x.Guid, x => Restore(x));
    }
}
