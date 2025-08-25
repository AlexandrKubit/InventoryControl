namespace Infrastructure.Services.Repositories;

using Domain.Entities.Directories;
using Infrastructure.Base;
using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;

internal class ClientRepository : BaseRepository<Client>, Client.IRepository
{
    private Context context { get; set; }

    public ClientRepository(Context context)
    {
        this.context = context;
    }

    public async Task FillByNames(List<string> names)
    {
        var func = async (IEnumerable<string> args) =>
            await context.Clients.Where(x => args.Contains(x.Name)).Select(x => x.Guid).ToListAsync();

        await LoadWithCacheAsync(names, func, this);
    }

    protected override async Task Commit()
    {
        await EntityCommitHelper.CommitEntities(
            dbSet: context.Clients,
            entities: list,
            createMapDelegate: entity => new Entities.Client
            {
                Guid = entity.Guid,
                Name = entity.Name,
                Address = entity.Address,
                Condition = entity.Condition
            },
            updateMapDelegate: (dbEntity, entity) =>
            {
                dbEntity.Name = entity.Name;
                dbEntity.Address = entity.Address;
                dbEntity.Condition = entity.Condition;
            }
        );
        await context.SaveChangesAsync();
    }

    protected override async Task<List<Client>> GetFromDbByIdsAsync(List<Guid> guids)
    {
        return await context.Clients
            .Where(x => guids.Contains(x.Guid))
            .Select(x => Client.IRepository.Restore(x.Guid, x.Name, x.Address, x.Condition))
            .ToListAsync();
    }
}
