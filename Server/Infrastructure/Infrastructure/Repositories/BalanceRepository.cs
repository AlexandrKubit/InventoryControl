namespace Infrastructure.Repositories;

using Domain.Entities.Warehouse;
using Infrastructure.Base;
using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;

internal class BalanceRepository : BaseRepository<Balance>, Balance.IRepository
{
    public BalanceRepository(UnitOfWork uow)
    {
        context = uow.Context;
    }

    private readonly Context context;

    private Balance Restore(Entities.Balance balance)
    {
        return Balance.IRepository.Restore(balance.Guid, balance.ResourceGuid, balance.MeasureUnitGuid, balance.Quantity);
    }


    public async Task EnsureByMeasureUnitGuids(HashSet<Guid> unitGuids)
    {
        var func = async (HashSet<Guid> guids) =>
            await context.Balances
                .Where(x => guids.Contains(x.MeasureUnitGuid))
                .Where(x => !LoadedGuids.Contains(x.Guid))
                .ToDictionaryAsync(x => x.Guid, x => Restore(x));

        await LoadWithCacheAsync(unitGuids, func);
    }

    public async Task EnsureByResourceGuids(HashSet<Guid> resourceGuids)
    {
        var func = async (HashSet<Guid> guids) =>
             await context.Balances
                .Where(x => guids.Contains(x.ResourceGuid))
                .Where(x => !LoadedGuids.Contains(x.Guid))
                .ToDictionaryAsync(x => x.Guid, x => Restore(x));

        await LoadWithCacheAsync(resourceGuids, func);
    }

    public async Task EnsureByResourceMeasureUnit(HashSet<(Guid ResourceGuid, Guid MeasureUnitGuid)> args)
    {
        var compositeKeys = args.Select(a => $"{a.ResourceGuid}:{a.MeasureUnitGuid}").ToHashSet();

        var func = async (IEnumerable<string> args) =>
            await context.Balances
                .Where(x => args.Contains(x.ResourceGuid.ToString() + ":" + x.MeasureUnitGuid.ToString()))
                .Where(x => !LoadedGuids.Contains(x.Guid))
                .ToDictionaryAsync(x => x.Guid, x => Restore(x));

        await LoadWithCacheAsync(compositeKeys, func);
    }

    public override void Commit()
    {
        EntityCommitHelper.CommitEntities(
            dbSet: context.Balances,
            entities: collection.Values,
            createMapDelegate: entity => new Entities.Balance
            {
                Guid = entity.Guid,
                MeasureUnitGuid = entity.MeasureUnitGuid,
                ResourceGuid = entity.ResourceGuid,
                Quantity = entity.Quantity
            },
            updateMapDelegate: (dbEntity, entity) =>
            {
                dbEntity.MeasureUnitGuid = entity.MeasureUnitGuid;
                dbEntity.ResourceGuid = entity.ResourceGuid;
                dbEntity.Quantity = entity.Quantity;
            }
        );
    }

    protected override async Task<Dictionary<Guid, Balance>> GetFromDbByGuidsAsync(HashSet<Guid> guids)
    {
        return await context.Balances
            .Where(x => guids.Contains(x.Guid))
            .ToDictionaryAsync(x => x.Guid, x => Restore(x));
    }
}
