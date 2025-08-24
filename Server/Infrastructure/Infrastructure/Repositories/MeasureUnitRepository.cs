namespace Infrastructure.Repositories;

using Domain.Entities.Directories;
using Infrastructure.Base;
using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;

internal class MeasureUnitRepository : BaseRepository<MeasureUnit>, MeasureUnit.IRepository
{
    public MeasureUnitRepository(Context context)
    {
        this.context = context;
    }

    private Context context { get; set; }


    public async Task FillByNames(List<string> names)
    {
        var func = async (IEnumerable<string> args) =>
            await context.MeasureUnits.Where(x => args.Contains(x.Name)).Select(x => x.Guid).ToListAsync();
        await LoadWithCacheAsync(names, func, this);
    }

    protected override async Task Commit()
    {
        await EntityCommitHelper.CommitEntities(
            dbSet: context.MeasureUnits,
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
        await context.SaveChangesAsync();
    }

    protected override async Task<List<MeasureUnit>> GetFromDbByIdsAsync(List<Guid> guids)
    {
        return await context.MeasureUnits
            .Where(x => guids.Contains(x.Guid))
            .Select(x => MeasureUnit.IRepository.Restore(x.Guid, x.Name, x.Condition))
            .ToListAsync();
    }
}
