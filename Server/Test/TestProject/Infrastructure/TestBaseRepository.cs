using Domain.Base;

namespace TestProject.Infrastructure;
public abstract class TestBaseRepository<TEntity> where TEntity : BaseEntity
{
    protected Dictionary<Guid, TEntity> collection = new();
    public IEnumerable<TEntity> List => collection
        .Select(x => x.Value)
        .Where(x => x.ModificationType != BaseEntity.ModificationTypes.Removed);

    public void Add(TEntity entity)
    {
        if (entity.ModificationType == BaseEntity.ModificationTypes.Created)
            collection.Add(entity.Guid, entity);
    }

    public abstract void InitData();
}
