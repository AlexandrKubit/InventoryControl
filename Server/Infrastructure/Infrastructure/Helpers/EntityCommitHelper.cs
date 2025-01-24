namespace Infrastructure.Helpers;

using Domain.Base;
using Infrastructure.Base;
using Microsoft.EntityFrameworkCore;

public static class EntityCommitHelper
{
    /// <summary>
    /// Метод удобен для использования при реализации метода commit в репозитории.
    /// Его назначение - вынести однотипные повторяющиеся действия из репозитория.
    /// </summary>
    /// <typeparam name="TEntity">Доменная сущность</typeparam>
    /// <typeparam name="TEntityMap">Сущность из контексата EF</typeparam>
    /// <param name="dbSet">Класс, отражающий таблицу БД</param>
    /// <param name="entities">Список доменных сущностей для сохранения в БД</param>
    /// <param name="createMapDelegate">Делегат на метод который умеет создавать entityMap из entity</param>
    /// <param name="updateMapDelegate">Делегат на метод по изменению entityMap</param>
    /// <returns></returns>
    public static async Task CommitEntities<TEntity, TEntityMap>(
        DbSet<TEntityMap> dbSet,
        IEnumerable<TEntity> entities,
        Func<TEntity, TEntityMap> createMapDelegate,
        Action<TEntityMap, TEntity> updateMapDelegate)
        where TEntity : BaseEntity
        where TEntityMap : class, IGuidIdentity
    {
        
        // Добавление новых
        var created = entities.Where(x => x.ModificationType == BaseEntity.ModificationTypes.Created);
        dbSet.AddRange(created.Select(createMapDelegate));

        // Обновление существующих
        var modified = entities.Where(x => x.ModificationType == BaseEntity.ModificationTypes.Updated);
        var modifiedIds = modified.Select(x => x.Guid).ToList();
        var inDb = await dbSet.Where(x => modifiedIds.Contains(x.Guid)).ToListAsync();

        foreach (var dbEntity in inDb)
        {
            var entity = modified.First(x => x.Guid == dbEntity.Guid);
            updateMapDelegate(dbEntity, entity);
        }

        // Удаление сущностей
        var deleted = entities.Where(x => x.ModificationType == BaseEntity.ModificationTypes.Removed);
        var deletedIds = deleted.Select(x => x.Guid).ToList();
        var toDelete = dbSet.Where(x => deletedIds.Contains(x.Guid));

        dbSet.RemoveRange(toDelete);
    }
}
