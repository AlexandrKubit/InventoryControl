namespace Infrastructure.Repositories;

using Domain.Entities.Warehouse.Receipt;
using Infrastructure.Base;
using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;

internal class ReceiptItemRepository : BaseRepository<Item>, Item.IRepository
{
    public ReceiptItemRepository(UnitOfWork uow)
    {
        this.context = uow.Context;
    }

    private Context context { get; set; }

    public async Task FillByMeasureUnitGuids(List<Guid> unitGuids)
    {
        var func = async (IEnumerable<Guid> guids) => 
            await context.ReceiptItems.Where(x => guids.Contains(x.MeasureUnitGuid)).Select(x => x.Guid).ToListAsync();
        await LoadWithCacheAsync(unitGuids, func, this);
    }

    public async Task FillByReceiptGuids(List<Guid> receiptGuids)
    {
        var func = async (IEnumerable<Guid> guids) => 
            await context.ReceiptItems.Where(x => guids.Contains(x.ReceiptGuid)).Select(x => x.Guid).ToListAsync();
        await LoadWithCacheAsync(receiptGuids, func, this);
    }

    public async Task FillByResourceGuids(List<Guid> resourceGuids)
    {
        var func = async (IEnumerable<Guid> guids) => 
            await context.ReceiptItems.Where(x => guids.Contains(x.ResourceGuid)).Select(x => x.Guid).ToListAsync();
        await LoadWithCacheAsync(resourceGuids, func, this);
    }

    public override void Commit()
    {
        EntityCommitHelper.CommitEntities(
            dbSet: context.ReceiptItems,
            entities: list,
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

    protected override async Task<List<Item>> GetFromDbByIdsAsync(List<Guid> guids)
    {
        return (await context.ReceiptItems
            .Where(x => guids.Contains(x.Guid))
            .ToListAsync())     
            .Select(x => Item.IRepository.Restore(x.Guid, x.ReceiptGuid, x.ResourceGuid, x.MeasureUnitGuid, x.Quantity))
            .ToList();
    }
}
