namespace Infrastructure.Repositories;

using Domain.Entities.Directories;
using Infrastructure.Base;
using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using System;

internal class ResourceRepository : BaseRepository<Resource>, Resource.IRepository
{
    public ResourceRepository(UnitOfWork uow)
    {
        this.context = uow.Context;
    }

    private Context context { get; set; }


    public async Task FillByNames(List<string> names)
    {
        var func = async (IEnumerable<string> args) =>
            await context.Resources.Where(x => args.Contains(x.Name)).Select(x => x.Guid).ToListAsync();
        await LoadWithCacheAsync(names, func, this);
    }

    public override void Commit()
    {
        EntityCommitHelper.CommitEntities(
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
    }

    protected override async Task<List<Resource>> GetFromDbByGuidsAsync(List<Guid> guids)
    {
        return (await context.Resources
            .Where(x => guids.Contains(x.Guid))
            .ToListAsync())
            .Select(x => Resource.IRepository.Restore(x.Guid, x.Name, x.Condition))
            .ToList();
    }
}
