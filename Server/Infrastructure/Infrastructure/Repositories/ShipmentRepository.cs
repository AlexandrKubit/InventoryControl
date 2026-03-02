namespace Infrastructure.Repositories;

using Domain.Entities.Warehouse.Shipment;
using Infrastructure.Base;
using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;

internal class ShipmentRepository : BaseRepository<Document>, Document.IRepository
{
    public ShipmentRepository(UnitOfWork uow)
    {
        this.context = uow.Context;
    }

    private Context context { get; set; }

    public async Task FillByClients(List<Guid> clientGuids)
    {
        var func = async (IEnumerable<Guid> args) =>
            await context.Shipments.Where(x => args.Contains(x.ClientGuid)).Select(x => x.Guid).ToListAsync();
        await LoadWithCacheAsync(clientGuids, func, this);
    }

    public async Task FillByNumbers(List<string> numbers)
    {
        var func = async (IEnumerable<string> args) =>
            await context.Shipments.Where(x => args.Contains(x.Number)).Select(x => x.Guid).ToListAsync();
        await LoadWithCacheAsync(numbers, func, this);
    }


    public override void Commit()
    {
        EntityCommitHelper.CommitEntities(
            dbSet: context.Shipments,
            entities: list,
            createMapDelegate: entity => new Entities.Shipment
            {
                Guid = entity.Guid,
                Number = entity.Number,
                Date = entity.Date.ToUniversalTime(),
                ClientGuid = entity.ClientGuid,
                Condition = entity.Condition
            },
            updateMapDelegate: (dbEntity, entity) =>
            {
                dbEntity.Number = entity.Number;
                dbEntity.Date = entity.Date.ToUniversalTime();
                dbEntity.ClientGuid = entity.ClientGuid;
                dbEntity.Condition = entity.Condition;
            }
        );
    }

    protected override async Task<List<Document>> GetFromDbByIdsAsync(List<Guid> guids)
    {
        return (await context.Shipments
            .Where(x => guids.Contains(x.Guid))
            .ToListAsync())      
            .Select(x => Document.IRepository.Restore(x.Guid, x.Number, x.ClientGuid, x.Date, x.Condition))
            .ToList();
    }
}
