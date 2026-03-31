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

    /// <summary>
    /// Декларативная загрузка: домен запрашивает данные по именам.
    /// Метод использует универсальный кэширующий механизм LoadWithCacheAsync,
    /// который гарантирует, что каждый уникальный набор аргументов будет загружен из БД только один раз
    /// в рамках жизненного цикла репозитория (и, соответственно, UoW).
    /// </summary>
    public async Task FillByNames(List<string> names)
    {
        var func = async (IEnumerable<string> args) =>
            await context.Clients.Where(x => args.Contains(x.Name)).Select(x => x.Guid).ToListAsync();

        await LoadWithCacheAsync(names, func, this);
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
    }

    /// <summary>
    /// Технический метод загрузки сущностей из БД по списку идентификаторов.
    /// Возвращает доменные объекты, используя protected статический метод Restore,
    /// объявленный в Client.IRepository. Это единственный способ восстановить сущность
    /// из БД, не нарушая её инкапсуляцию (конструктор приватный).
    /// </summary>
    protected override async Task<List<Client>> GetFromDbByGuidsAsync(List<Guid> guids)
    {
        return (await context.Clients
            .Where(x => guids.Contains(x.Guid))
            .ToListAsync())
            .Select(x => Client.IRepository.Restore(x.Guid, x.Name, x.Address, x.Condition))
            .ToList();
    }
}
