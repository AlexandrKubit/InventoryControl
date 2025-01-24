namespace Infrastructure.Services.Repositories;

using Domain.Entities.Directories;
using Infrastructure.Base;
using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;

internal class ClientRepository : BaseRepository<Client>, App.Commands.Repositories.IClientRepository
{
    public ClientRepository(Context context)
    {
        Context = context;
    }

    private Context Context { get; set; }


    public async Task FillByNames(List<string> names)
    {
        var func = async (IEnumerable<string> args) =>
            await Context.Clients.Where(x => args.Contains(x.Name)).Select(x => x.Guid).ToListAsync();

        await LoadWithCacheAsync(names, func);
    }

    protected override async Task Commit()
    {
        await EntityCommitHelper.CommitEntities(
            dbSet: Context.Clients,
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
        await Context.SaveChangesAsync();
    }

    protected override async Task<List<Client>> GetFromDbByIdsAsync(List<Guid> guids)
    {
        return await Context.Clients
            .Where(x => guids.Contains(x.Guid))
            .Select(x => Client.IRepository.Restore(x.Guid, x.Name, x.Address, x.Condition))
            .ToListAsync();
    }
}
