namespace Infrastructure.Repositories;

using Domain.Entities.Directories;
using Infrastructure.Base;
using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;

internal class MeasureUnitRepository : BaseRepository<MeasureUnit>, MeasureUnit.IRepository
{
    public MeasureUnitRepository(UnitOfWork uow)
    {
        this.context = uow.Context;
    }

    private Context context { get; set; }

    private MeasureUnit Restore(Entities.MeasureUnit unit) =>
        MeasureUnit.IRepository.Restore(unit.Guid, unit.Name, unit.Condition);

    public async Task EnsureByNames(HashSet<string> names)
    {
        var func = async (IEnumerable<string> args) =>
            await context.MeasureUnits
                .Where(x => args.Contains(x.Name))
				.Where(x => !LoadedGuids.Contains(x.Guid))
				.ToDictionaryAsync(x => x.Guid, x => Restore(x));

		await LoadWithCacheAsync(names, func);
    }

    public override void Commit()
    {
        EntityCommitHelper.CommitEntities(
            dbSet: context.MeasureUnits,
            entities: collection.Values,
            createMapDelegate: entity => new Entities.MeasureUnit
            {
                Guid = entity.Guid,
                Name = entity.Name,
                Condition = entity.Condition
            },
            updateMapDelegate: (dbEntity, entity) =>
            {
                dbEntity.Name = entity.Name;
                dbEntity.Condition = entity.Condition;
            }
        );
    }

    protected override async Task<Dictionary<Guid, MeasureUnit>> GetFromDbByGuidsAsync(HashSet<Guid> guids)
    {
        return await context.MeasureUnits
			.Where(x => guids.Contains(x.Guid))
			.ToDictionaryAsync(x => x.Guid, x => Restore(x));
	}
}
