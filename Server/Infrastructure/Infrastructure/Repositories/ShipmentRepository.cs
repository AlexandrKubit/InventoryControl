namespace Infrastructure.Repositories;

using Domain.Entities.Warehouse.Shipment;
using Infrastructure.Base;
using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;

internal class ShipmentRepository : BaseRepository<Document>, App.Commands.Repositories.IShipmentRepository
{
    public ShipmentRepository(Context context)
    {
        Context = context;
    }

    private Context Context { get; set; }

    public async Task FillByClients(List<Guid> clientGuids)
    {
        var func = async (IEnumerable<Guid> args) =>
            await Context.Shipments.Where(x => args.Contains(x.ClientGuid)).Select(x => x.Guid).ToListAsync();
        await LoadWithCacheAsync(clientGuids, func, this);
    }

    public async Task FillByNumbers(List<string> numbers)
    {
        var func = async (IEnumerable<string> args) =>
            await Context.Shipments.Where(x => args.Contains(x.Number)).Select(x => x.Guid).ToListAsync();
        await LoadWithCacheAsync(numbers, func, this);
    }


    protected override async Task Commit()
    {
        await EntityCommitHelper.CommitEntities(
            dbSet: Context.Shipments,
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
        await Context.SaveChangesAsync();
    }

    protected override async Task<List<Document>> GetFromDbByIdsAsync(List<Guid> guids)
    {
        return await Context.Shipments
            .Where(x => guids.Contains(x.Guid))
            .Select(x => Document.IRepository.Restore(x.Guid, x.Number, x.ClientGuid, x.Date, x.Condition))
            .ToListAsync();            
    }
}
