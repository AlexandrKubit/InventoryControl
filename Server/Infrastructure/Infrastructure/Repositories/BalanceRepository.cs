namespace Infrastructure.Repositories;

using Domain.Entities.Warehouse;
using Infrastructure.Base;
using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;

internal class BalanceRepository : BaseRepository<Balance>, App.Commands.Repositories.IBalanceRepository
{
    public BalanceRepository(Context context)
    {
        Context = context;
    }

    private Context Context { get; set; }

    public async Task FillByMeasureUnitGuids(List<Guid> unitGuids)
    {
        var func = async (IEnumerable<Guid> guids) =>
            await Context.Balances.Where(x => guids.Contains(x.MeasureUnitGuid)).Select(x => x.Guid).ToListAsync();
        await LoadWithCacheAsync(unitGuids, func, this);
    }

    public async Task FillByResourceGuids(List<Guid> resourceGuids)
    {
        var func = async (IEnumerable<Guid> guids) =>
             await Context.Balances.Where(x => guids.Contains(x.ResourceGuid)).Select(x => x.Guid).ToListAsync();
        await LoadWithCacheAsync(resourceGuids, func, this);
    }

    public async Task FillByResourceMeasureUnit(IEnumerable<(Guid ResourceGuid, Guid MeasureUnitGuid)> args)
    {
        var compositeKeys = args.Select(a => $"{a.ResourceGuid}:{a.MeasureUnitGuid}").ToList();

        var func = async (IEnumerable<string> args) =>
            await Context.Balances
            .Where(b => args.Contains(b.ResourceGuid.ToString() + ":" + b.MeasureUnitGuid.ToString()))
            .Select(b => b.Guid)
            .ToListAsync();

        await LoadWithCacheAsync(compositeKeys, func, this);
    }

    protected override async Task Commit()
    {
        await EntityCommitHelper.CommitEntities(
            dbSet: Context.Balances,
            entities: list,
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
        await Context.SaveChangesAsync();
    }

    protected override async Task<List<Balance>> GetFromDbByIdsAsync(List<Guid> guids)
    {
        return await Context.Balances
            .Where(x => guids.Contains(x.Guid))
            .Select(x => Balance.IRepository.Restore(x.Guid, x.ResourceGuid, x.MeasureUnitGuid, x.Quantity))
            .ToListAsync();
    }
}
