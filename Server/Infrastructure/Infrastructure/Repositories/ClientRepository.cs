namespace Infrastructure.Services.Repositories;

using Domain.Entities.Directories;
using Infrastructure.Base;
using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;

internal class ClientRepository : BaseRepository<Client>, Client.IRepository
{
    private Context context { get; set; }

    public ClientRepository(UnitOfWork uow)
    {
        this.context = uow.Context;
    }

    /// использует статический protected метод Restore, объявленный в Client.IRepository 
    /// единственный способ восстановить сущность из БД, не нарушая её инкапсуляцию (конструктор приватный)
    private Client Restore(Entities.Client client)
    {
        return Client.IRepository.Restore(client.Guid, client.Name, client.Address, client.Condition);
    }

    /// <summary>
    /// Декларативная загрузка: домен запрашивает данные по именам.
    /// Метод использует универсальный кэширующий механизм LoadWithCacheAsync,
    /// который гарантирует, что каждый уникальный набор аргументов будет загружен из БД только один раз
    /// в рамках жизненного цикла репозитория (и, соответственно, UoW).
    /// </summary>
    public async Task EnsureByNames(HashSet<string> names)
    {
        var func = async (HashSet<string> args) =>
            await context.Clients
                .Where(x => args.Contains(x.Name))
                .Where(x => !LoadedGuids.Contains(x.Guid)) // загружаем только те сущности, которых нет в словаре
                .ToDictionaryAsync(x => x.Guid, x => Restore(x));

        await LoadWithCacheAsync(names, func);
    }

    /// <summary>
    /// Фиксация изменений: хэлпер EntityCommitHelper обрабатывает все сущности из коллекции list,
    /// анализируя их ModificationType (Created, Updated, Removed), и применяет соответствующие
    /// операции к DbSet context.Clients. Это позволяет сохранить атомарность и избавляет
    /// домен от явных вызовов репозитория для сохранения.
    /// </summary>
    public override void Commit()
    {
        EntityCommitHelper.CommitEntities(
            dbSet: context.Clients,
            entities: collection.Values,
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
    }

    /// <summary>
    /// Технический метод загрузки сущностей из БД по списку идентификаторов.
    /// </summary>
    protected override async Task<Dictionary<Guid, Client>> GetFromDbByGuidsAsync(HashSet<Guid> guids)
    {
        return await context.Clients
            .Where(x => guids.Contains(x.Guid))
            .ToDictionaryAsync(x => x.Guid, x => Restore(x));
    }
}
