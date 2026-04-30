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

    private Document Restore(Entities.Receipt receipt) =>
        Document.IRepository.Restore(receipt.Guid, receipt.Number, receipt.Date);

    public async Task EnsureByNumbers(HashSet<string> numbers)
    {
        var func = async (IEnumerable<string> args) =>
             await context.Receipts
                 .Where(x => args.Contains(x.Number))
                 .Where(x => !LoadedGuids.Contains(x.Guid))
                 .ToDictionaryAsync(x => x.Guid, x => Restore(x));

        await LoadWithCacheAsync(numbers, func);
    }


    public override void Commit()
    {
        EntityCommitHelper.CommitEntities(
            dbSet: context.Receipts,
            entities: collection.Values,
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

    protected override async Task<Dictionary<Guid,Document>> GetFromDbByGuidsAsync(HashSet<Guid> guids)
    {
        return await context.Receipts
            .Where(x => guids.Contains(x.Guid))
            .ToDictionaryAsync(x => x.Guid, x => Restore(x));
    }
}
