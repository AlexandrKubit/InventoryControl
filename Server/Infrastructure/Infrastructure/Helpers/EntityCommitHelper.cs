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
        where TEntityMap : class, IGuidIdentity, new()
    {
        // 1. Добавление новых
        var created = entities.Where(x => x.ModificationType == BaseEntity.ModificationTypes.Created);
        dbSet.AddRange(created.Select(createMapDelegate));

        // 2. Обновление существующих через Attach
        var modified = entities.Where(x => x.ModificationType == BaseEntity.ModificationTypes.Updated);
        foreach (var entity in modified)
        {
            // такой подход позволяет использовать EF Core без change tracking
            // ВАЖНО: необходимо создать пустой объект, прикрепить его к контексту, а затем обновить нужные поля
            var dbEntity = new TEntityMap { Guid = entity.Guid };
            dbSet.Attach(dbEntity);        // "Прикрепляем" к контексту как существующий
            updateMapDelegate(dbEntity, entity); // Применяем изменения из домена
        }

        // 3. Удаление существующих через Attach и Remove
        var deleted = entities.Where(x => x.ModificationType == BaseEntity.ModificationTypes.Removed);
        foreach (var entity in deleted)
        {
            var dbEntity = createMapDelegate(entity);
            dbSet.Attach(dbEntity); // Прикрепляем как существующий
            dbSet.Remove(dbEntity); // Помечаем на удаление
        }
    }
}
