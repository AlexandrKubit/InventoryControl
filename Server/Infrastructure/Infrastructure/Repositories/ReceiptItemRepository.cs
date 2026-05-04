namespace Infrastructure.Repositories;

using Domain.Entities.Warehouse.Receipt;
using Infrastructure.Base;
using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;

internal class ReceiptItemRepository : BaseRepository<Item>, Item.IRepository
{
    public ReceiptItemRepository(UnitOfWork uow)
    {
        context = uow.Context;
    }

    private readonly Context context;

    private Item Restore(Entities.ReceiptItem item) =>
        Item.IRepository.Restore(item.Guid, item.ReceiptGuid, item.ResourceGuid, item.MeasureUnitGuid, item.Quantity);

    public async Task EnsureByMeasureUnitGuids(HashSet<Guid> unitGuids)
    {
        var func = async (IEnumerable<Guid> guids) => 
            await context.ReceiptItems
                .Where(x => guids.Contains(x.MeasureUnitGuid))
				.Where(x => !LoadedGuids.Contains(x.Guid))
				.ToDictionaryAsync(x => x.Guid, x => Restore(x));

		await LoadWithCacheAsync(unitGuids, func);
    }

    public async Task EnsureByReceiptGuids(HashSet<Guid> receiptGuids)
    {
        var func = async (IEnumerable<Guid> guids) => 
            await context.ReceiptItems
                .Where(x => guids.Contains(x.ReceiptGuid))
				.Where(x => !LoadedGuids.Contains(x.Guid))
				.ToDictionaryAsync(x => x.Guid, x => Restore(x));

		await LoadWithCacheAsync(receiptGuids, func);
    }

    public async Task EnsureByResourceGuids(HashSet<Guid> resourceGuids)
    {
        var func = async (IEnumerable<Guid> guids) => 
            await context.ReceiptItems
                .Where(x => guids.Contains(x.ResourceGuid))
				.Where(x => !LoadedGuids.Contains(x.Guid))
				.ToDictionaryAsync(x => x.Guid, x => Restore(x));

		await LoadWithCacheAsync(resourceGuids, func);
    }

    public override void Commit()
    {
        EntityCommitHelper.CommitEntities(
            dbSet: context.ReceiptItems,
            entities: Collection.Values,
            createMapDelegate: entity => new Entities.ReceiptItem
            {
                Guid = entity.Guid,
                ReceiptGuid = entity.ReceiptGuid,
                ResourceGuid = entity.ResourceGuid,
                MeasureUnitGuid = entity.MeasureUnitGuid,
                Quantity = entity.Quantity
            },
            updateMapDelegate: (dbEntity, entity) =>
            {
                dbEntity.ReceiptGuid = entity.ReceiptGuid;
                dbEntity.ResourceGuid = entity.ResourceGuid;
                dbEntity.MeasureUnitGuid = entity.MeasureUnitGuid;
                dbEntity.Quantity = entity.Quantity;
            }
        );
    }

    protected override async Task<Dictionary<Guid, Item>> GetFromDbByGuidsAsync(HashSet<Guid> guids)
    {
        return await context.ReceiptItems
			.Where(x => guids.Contains(x.Guid))
			.ToDictionaryAsync(x => x.Guid, x => Restore(x));
	}
}
