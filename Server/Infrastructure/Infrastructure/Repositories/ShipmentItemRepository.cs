namespace Infrastructure.Repositories;

using Domain.Entities.Warehouse.Shipment;
using Infrastructure.Base;
using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;

internal class ShipmentItemRepository : BaseRepository<Item>, App.Commands.Repositories.IShipmentItemRepository
{
    public ShipmentItemRepository(Context context)
    {
        Context = context;
    }

    private Context Context { get; set; }

    public async Task FillByMeasureUnitGuids(List<Guid> unitGuids)
    {

        var func = async (IEnumerable<Guid> args) =>
            await Context.ShipmentItems.Where(x => args.Contains(x.MeasureUnitGuid)).Select(x => x.Guid).ToListAsync();
        await LoadWithCacheAsync(unitGuids, func, this);
    }

    public async Task FillByShipmentGuids(List<Guid> shipmentGuids)
    {
        var func = async (IEnumerable<Guid> args) =>
            await Context.ShipmentItems.Where(x => args.Contains(x.ShipmentGuid)).Select(x => x.Guid).ToListAsync();

        await LoadWithCacheAsync(shipmentGuids, func, this);
    }

    public async Task FillByResourceGuids(List<Guid> resourceGuids)
    {
        var func = async (IEnumerable<Guid> args) =>
            await Context.ShipmentItems.Where(x => args.Contains(x.ResourceGuid)).Select(x => x.Guid).ToListAsync();
        await LoadWithCacheAsync(resourceGuids, func, this);
    }

    protected override async Task Commit()
    {
        await EntityCommitHelper.CommitEntities(
            dbSet: Context.ShipmentItems,
            entities: list,
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
        await Context.SaveChangesAsync();
    }

    protected override async Task<List<Item>> GetFromDbByIdsAsync(List<Guid> guids)
    {
        return await Context.ShipmentItems
            .Where(x => guids.Contains(x.Guid))
            .Select(x => Item.IRepository.Restore(x.Guid, x.ShipmentGuid, x.ResourceGuid, x.MeasureUnitGuid, x.Quantity))
            .ToListAsync();            
    }
}
