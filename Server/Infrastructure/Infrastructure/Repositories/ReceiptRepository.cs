namespace Infrastructure.Repositories;

using Domain.Entities.Warehouse.Receipt;
using Infrastructure.Base;
using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;

internal class ReceiptRepository : BaseRepository<Document>, Document.IRepository
{
    public ReceiptRepository(UnitOfWork uow)
    {
        this.context = uow.Context;
    }

    private Context context { get; set; }
    

    public async Task FillByNumbers(List<string> numbers)
    {
        var func = async (IEnumerable<string> args) =>
             await context.Receipts.Where(x => args.Contains(x.Number)).Select(x => x.Guid).ToListAsync();
        await LoadWithCacheAsync(numbers, func, this);
    }


    public override void Commit()
    {
        EntityCommitHelper.CommitEntities(
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
    }

    protected override async Task<List<Document>> GetFromDbByGuidsAsync(List<Guid> guids)
    {
        return (await context.Receipts
            .Where(x => guids.Contains(x.Guid))
            .ToListAsync())     
            .Select(x => Document.IRepository.Restore(x.Guid, x.Number, x.Date))
            .ToList();
    }
}
