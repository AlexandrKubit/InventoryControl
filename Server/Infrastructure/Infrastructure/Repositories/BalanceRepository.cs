namespace Infrastructure.Repositories;

using App.Base.Mediator;
using Domain.Entities.Warehouse;
using Infrastructure.Base;
using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;

internal class BalanceRepository : BaseRepository<Balance>, Balance.IRepository
{
    public BalanceRepository(UnitOfWork uow)
    {
        this.context = uow.Context;
    }

    private Context context { get; set; }

    public async Task FillByMeasureUnitGuids(List<Guid> unitGuids)
    {
        var func = async (IEnumerable<Guid> guids) =>
            await context.Balances.Where(x => guids.Contains(x.MeasureUnitGuid)).Select(x => x.Guid).ToListAsync();
        await LoadWithCacheAsync(unitGuids, func, this);
    }

    public async Task FillByResourceGuids(List<Guid> resourceGuids)
    {
        var func = async (IEnumerable<Guid> guids) =>
             await context.Balances.Where(x => guids.Contains(x.ResourceGuid)).Select(x => x.Guid).ToListAsync();
        await LoadWithCacheAsync(resourceGuids, func, this);
    }

    public async Task FillByResourceMeasureUnit(IEnumerable<(Guid ResourceGuid, Guid MeasureUnitGuid)> args)
    {
        var compositeKeys = args.Select(a => $"{a.ResourceGuid}:{a.MeasureUnitGuid}").ToList();

        var func = async (IEnumerable<string> args) =>
            await context.Balances
            .Where(b => args.Contains(b.ResourceGuid.ToString() + ":" + b.MeasureUnitGuid.ToString()))
            .Select(b => b.Guid)
            .ToListAsync();

        await LoadWithCacheAsync(compositeKeys, func, this);
    }

    public override void Commit()
    {
        EntityCommitHelper.CommitEntities(
            dbSet: context.Balances,
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
    }

    protected override async Task<List<Balance>> GetFromDbByGuidsAsync(List<Guid> guids)
    {
        return (await context.Balances
            .Where(x => guids.Contains(x.Guid))
            .ToListAsync())
            .Select(x => Balance.IRepository.Restore(x.Guid, x.ResourceGuid, x.MeasureUnitGuid, x.Quantity))
            .ToList();
    }
}
