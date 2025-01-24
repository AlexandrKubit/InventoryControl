namespace Infrastructure.Services.Repositories;

using Domain.Entities.Warehouse.Receipt;
using Infrastructure.Base;
using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;

internal class ReceiptRepository : BaseRepository<Document>, App.Commands.Repositories.IReceiptRepository
{
    public ReceiptRepository(Context context)
    {
        Context = context;
    }

    private Context Context { get; set; }
    

    public async Task FillByNumbers(List<string> numbers)
    {
        var func = async (IEnumerable<string> args) =>
             await Context.Receipts.Where(x => args.Contains(x.Number)).Select(x => x.Guid).ToListAsync();
        await LoadWithCacheAsync(numbers, func);
    }


    protected override async Task Commit()
    {
        await EntityCommitHelper.CommitEntities(
            dbSet: Context.Receipts,
            entities: list,
            createMapDelegate: entity => new Entities.Receipt
            {
                Guid = entity.Guid,
                Number = entity.Number,
                Date = entity.Date
            },
            updateMapDelegate: (dbEntity, entity) =>
            {
                dbEntity.Number = entity.Number;
                dbEntity.Date = entity.Date;
            }
        );
        await Context.SaveChangesAsync();
    }

    protected override async Task<List<Document>> GetFromDbByIdsAsync(List<Guid> guids)
    {
        return await Context.Receipts
            .Where(x => guids.Contains(x.Guid))
            .Select(x => Document.IRepository.Restore(x.Guid, x.Number, x.Date))
            .ToListAsync();            
    }
}
