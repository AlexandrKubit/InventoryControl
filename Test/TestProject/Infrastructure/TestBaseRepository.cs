using Domain.Base;

namespace TestProject.Infrastructure;
public abstract class TestBaseRepository<TEntity> where TEntity : BaseEntity
{
    public IEnumerable<TEntity> List => list.Where(x => x.ModificationType != BaseEntity.ModificationTypes.Removed);
    protected List<TEntity> list = new();
    public void AddEntity(TEntity entity)
    {
        if (entity.ModificationType == BaseEntity.ModificationTypes.Created && !list.Any(e => e.Guid == entity.Guid))
        {
            list.Add(entity);
        }
    }
}
