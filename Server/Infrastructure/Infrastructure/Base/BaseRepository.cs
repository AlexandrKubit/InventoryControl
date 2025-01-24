namespace Infrastructure.Base;

using Domain.Base;

public abstract class BaseRepository<TEntity> where TEntity : BaseEntity
{
    /// <summary>
    /// Коллекция сущностей TEntity : BaseEntity, доступны лишь те, которые не помечены как "удаленные"
    /// </summary>
    public IEnumerable<TEntity> List => list.Where(x => x.ModificationType != BaseEntity.ModificationTypes.Removed);
    protected List<TEntity> list = new();

    /// <summary>
    /// Метод который добавлет сущность в коллекцию. 
    /// Вызывается из метода Append, но т.к. Append является protected и в нем сущность помечается как "созданная"
    /// (при этом больше никак не возможно отметить сущность как созданная),
    /// то вызов AddEntity из любого другого места теряет смысл, т.к. сущность просто не будет добавлена в коллекцию
    /// </summary>
    /// <param name="entity"></param>
    public void AddEntity(TEntity entity)
    {
        if (entity.ModificationType == BaseEntity.ModificationTypes.Created && !list.Any(e => e.Guid == entity.Guid))
        {
            list.Add(entity);
        }
    }

    ///// <summary>
    ///// коллекция функций и аргументов репозиториев, которые уже вызывались
    ///// </summary>
    private readonly Dictionary<string, HashSet<object>> cache = new();


    /// <summary>
    /// Метод сравнивает полученные guids с теми что уже добавлены в коллекцию
    /// и передает те, которых еще нет в коллекции в метод GetFromDbByIdsAsync,
    /// этот метод возвращает из БД сущности и потом эти сущности добавляются в коллекцию
    /// </summary>
    /// <param name="guids"></param>
    /// <returns></returns>
    private async Task FillCollection(List<Guid> guids)
    {
        var existGuids = list
            .Select(y => y.Guid)
            .ToList();

        guids = guids.Except(existGuids).ToList();

        if (guids.Count != 0)
        {
            var inDb = await GetFromDbByIdsAsync(guids);
            list.AddRange(inDb);
        }
    }

    /// <summary>
    /// Метод который возвращает список сущностей по guids из БД
    /// Его необходимо реализовывать в каждом репозитории
    /// Метод protected потому что его нельзя вызывать напрямую для получения сущностей из БД
    /// Так как напомню еще раз, репозиторий - это коллекция
    /// </summary>
    /// <param name="guids"></param>
    /// <returns></returns>
    protected abstract Task<List<TEntity>> GetFromDbByIdsAsync(List<Guid> guids);

    // метод, который часто встречается в сценариях и бизнес-действиях, поэтому добавлен по умолчанию
    public async Task FillByGuids(List<Guid> guids)
    {
        await FillCollection(guids);
    }

    /// <summary>
    /// Метод вызывается в конце сценария, для того чтобы синхронизировать данные в БД и данные в коллекции
    /// Условно если где то выбросилось исключение, то никакие данные даже не будут переданы в БД
    /// Нужно реализовывть в каждом репозитории
    /// В БД должны отправиться только те данные, которые были помечены как "создан", "изменен" или "удален"
    /// </summary>
    protected abstract Task Commit();


    ///// <summary>
    ///// Универсальный метод для кэшированной загрузки данных по методу и параметрам
    ///// если метод вызывался с этими же аргументами, то он не будет повторно вызываться
    ///// если присутствует новый аргумент, то метод вызовется только с новым аргументом
    ///// </summary>
    protected async Task LoadWithCacheAsync<TArgs>(IEnumerable<TArgs> args, Func<IEnumerable<TArgs>, Task<List<Guid>>> loadFunction)
    {
        var cacheKey = $"{loadFunction.Method.DeclaringType.FullName}:{loadFunction.Method.Name}";

        // Инициализация кэша для данного метода
        if (!cache.ContainsKey(cacheKey))
            cache[cacheKey] = new HashSet<object>();

        var cachedArgs = cache[cacheKey];

        // Определяем новые аргументы, которые ещё не обработаны
        var newArgs = args.Where(arg => !cachedArgs.Contains(arg)).ToList();
        if (!newArgs.Any()) return;

        // Добавляем новые аргументы в кэш
        foreach (var arg in newArgs)
        {
            cachedArgs.Add(arg);
        }

        // Вызываем загрузку только для новых аргументов
        var guids = await loadFunction(newArgs);
        await FillCollection(guids);
    }
}
