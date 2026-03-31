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
    public static void CommitEntities<TEntity, TEntityMap>(
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
        var inDb = dbSet.Local.Where(x => modifiedIds.Contains(x.Guid)).ToList();

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
