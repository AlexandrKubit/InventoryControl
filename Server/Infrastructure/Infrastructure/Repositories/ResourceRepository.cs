namespace Infrastructure.Repositories;

using Domain.Entities.Directories;
using Infrastructure.Base;
using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using System;

internal class ResourceRepository : BaseRepository<Resource>, Resource.IRepository
{
    public ResourceRepository(Context context)
    {
        this.context = context;
    }

    private Context context { get; set; }


    public async Task FillByNames(List<string> names)
    {
        var func = async (IEnumerable<string> args) =>
            await context.Resources.Where(x => args.Contains(x.Name)).Select(x => x.Guid).ToListAsync();
        await LoadWithCacheAsync(names, func, this);
    }

    protected override async Task Commit()
    {
        await EntityCommitHelper.CommitEntities(
            dbSet: context.Resources,
            entities: list,
            createMapDelegate: entity => new Entities.Resource
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

    protected override async Task<List<Resource>> GetFromDbByIdsAsync(List<Guid> guids)
    {
        return await context.Resources
            .Where(x => guids.Contains(x.Guid))
            .Select(x => Resource.IRepository.Restore(x.Guid, x.Name, x.Condition))
            .ToListAsync();
    }
}
