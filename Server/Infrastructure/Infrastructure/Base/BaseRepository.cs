namespace Infrastructure.Base;

using Domain.Base;
using System.Runtime.CompilerServices;

internal abstract class BaseRepository
{
    /// <summary>
    /// Метод вызывается в конце сценария, для того чтобы синхронизировать данные в БД и данные в коллекции
    /// Условно если где то выбросилось исключение, то никакие данные даже не будут переданы в БД
    /// Нужно реализовывать в каждом репозитории
    /// В БД должны отправиться только те данные, которые были помечены как "создан", "изменен" или "удален"
    /// </summary>
    public abstract void Commit();
}

internal abstract class BaseRepository<TEntity>: BaseRepository where TEntity: BaseEntity
{
    // основная коллеция сущностей выражена словарем для быстрого поиска по ключу
    protected Dictionary<Guid, TEntity> collection = new();

    /// <summary>
    /// Коллекция сущностей TEntity: BaseEntity, доступны лишь те, которые не помечены как "удаленные"
    /// </summary>
    public IEnumerable<TEntity> List => collection
        .Select(x=> x.Value)
        .Where(x => x.ModificationType != BaseEntity.ModificationTypes.Removed);

    // Намеренно создаем новый HashSet при обращении 
    // чтобы EF Core мог эффективно превратить Contains в SQL‑оператор IN
    protected HashSet<Guid> LoadedGuids => new(collection.Keys);


    /// <summary>
    /// Метод, который добавляет сущность в коллекцию. 
    /// Сущность должна быть помечена как Created
    /// А так как это может сделать только сама сущность то метод безопасен
    /// </summary>
    public void Add(TEntity entity)
    {
        if (entity.ModificationType == BaseEntity.ModificationTypes.Created)
            collection.Add(entity.Guid, entity);
    }


    /// <summary>Добавляет сущности, не вызывая исключений при уже существующих ключах.</summary>
    private void AddNewEntities(Dictionary<Guid, TEntity> entities)
    {
        foreach (var kvp in entities)
            collection.TryAdd(kvp.Key, kvp.Value);
    }

    /// <summary>
    /// Метод для быстрой загрузки по ключу, в обход общего кэша 
    /// сравнивает полученные guids с теми, что уже добавлены в коллекцию
    /// и передает те, которых еще нет в коллекции в метод GetFromDbByGuidsAsync,
    /// который возвращает сущности из БД чтобы добавить их в коллекцию
    /// </summary>
    public async Task EnsureByGuids(HashSet<Guid> guids)
    {
        var missing = guids.Where(g => !collection.ContainsKey(g)).ToHashSet();
        if (missing.Count == 0) 
            return;

        var loaded = await GetFromDbByGuidsAsync(missing);
        AddNewEntities(loaded);
    }

    /// <summary>
    /// Метод, который возвращает список сущностей по guids из БД
    /// </summary>
    protected abstract Task<Dictionary<Guid, TEntity>> GetFromDbByGuidsAsync(HashSet<Guid> guids);

    // кэш методов и аргументов репозиториев, которые уже вызывались 
    private readonly Dictionary<string, HashSet<object>> cache = new();

    protected async Task LoadWithCacheAsync<TArgs>(
        HashSet<TArgs> args,
        Func<HashSet<TArgs>, Task<Dictionary<Guid, TEntity>>> loadFunction,
        [CallerMemberName] string caller = "")
    {
        if (!cache.TryGetValue(caller, out var cachedArgs))
        {
            cachedArgs = new HashSet<object>();
            cache[caller] = cachedArgs;
        }

        // Определяем новые аргументы, которые ещё не обработаны
        var newArgs = args.Where(arg => !cachedArgs.Contains(arg)).ToHashSet();
        if (newArgs.Count == 0) return;

        // Добавляем новые аргументы в кэш
        foreach (var arg in newArgs)
            cachedArgs.Add(arg);

        // Вызываем загрузку только для новых аргументов
        var entities = await loadFunction(newArgs);

        AddNewEntities(entities);
    }
}
