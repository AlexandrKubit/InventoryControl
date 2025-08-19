namespace Infrastructure.Repositories;

using Domain.Entities.Directories;
using Infrastructure.Base;
using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;

internal class MeasureUnitRepository : BaseRepository<MeasureUnit>, MeasureUnit.IRepository
{
    public MeasureUnitRepository(Context context)
    {
        Context = context;
    }

    private Context Context { get; set; }


    public async Task FillByNames(List<string> names)
    {
        var func = async (IEnumerable<string> args) =>
            await Context.MeasureUnits.Where(x => args.Contains(x.Name)).Select(x => x.Guid).ToListAsync();
        await LoadWithCacheAsync(names, func, this);
    }

    protected override async Task Commit()
    {
        await EntityCommitHelper.CommitEntities(
            dbSet: Context.MeasureUnits,
            entities: list,
            createMapDelegate: entity => new Entities.MeasureUnit
            {
                Guid = entity.Guid,
                Name = entity.Name,
                Condition = entity.Condition
            },
            updateMapDelegate: (dbEntity, entity) =>
            {
                dbEntity.Name = entity.Name;
                dbEntity.Condition = entity.Condition;
            }
        );
        await Context.SaveChangesAsync();
    }

    protected override async Task<List<MeasureUnit>> GetFromDbByIdsAsync(List<Guid> guids)
    {
        return await Context.MeasureUnits
            .Where(x => guids.Contains(x.Guid))
            .Select(x => MeasureUnit.IRepository.Restore(x.Guid, x.Name, x.Condition))
            .ToListAsync();
    }
}
