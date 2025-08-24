namespace Infrastructure.Repositories;

using Domain.Entities.Warehouse.Receipt;
using Infrastructure.Base;
using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;

internal class ReceiptRepository : BaseRepository<Document>, Document.IRepository
{
    public ReceiptRepository(Context context)
    {
        this.context = context;
    }

    private Context context { get; set; }
    

    public async Task FillByNumbers(List<string> numbers)
    {
        var func = async (IEnumerable<string> args) =>
             await context.Receipts.Where(x => args.Contains(x.Number)).Select(x => x.Guid).ToListAsync();
        await LoadWithCacheAsync(numbers, func, this);
    }


    protected override async Task Commit()
    {
        await EntityCommitHelper.CommitEntities(
            dbSet: context.Receipts,
            entities: list,
            createMapDelegate: entity => new Entities.Receipt
            {
                Guid = entity.Guid,
                Number = entity.Number,
                Date = entity.Date.ToUniversalTime()
            },
            updateMapDelegate: (dbEntity, entity) =>
            {
                dbEntity.Number = entity.Number;
                dbEntity.Date = entity.Date.ToUniversalTime();
            }
        );
        await context.SaveChangesAsync();
    }

    protected override async Task<List<Document>> GetFromDbByIdsAsync(List<Guid> guids)
    {
        return await context.Receipts
            .Where(x => guids.Contains(x.Guid))
            .Select(x => Document.IRepository.Restore(x.Guid, x.Number, x.Date))
            .ToListAsync();            
    }
}
