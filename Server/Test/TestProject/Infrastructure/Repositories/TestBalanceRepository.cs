using Domain.Entities.Warehouse;
using TestProject.Infrastructure;

namespace Tests.Infrastructure;

internal class TestBalanceRepository : TestBaseRepository<Balance>, Balance.IRepository
{
    // по сути это для интеграциооных тестов
    public override void InitData()
    {
    }

    // а это для юнит тестов
    public void Add(Guid guid, Guid resourceGuid, Guid measureUnitGuid, decimal quantity) =>
        collection.Add(guid, Balance.IRepository.Restore(guid, resourceGuid, measureUnitGuid, quantity));

    public Task EnsureByResourceMeasureUnit(HashSet<(Guid ResourceGuid, Guid MeasureUnitGuid)> args) => Task.CompletedTask;
    public Task EnsureByMeasureUnitGuids(HashSet<Guid> unitGuids) => Task.CompletedTask;
    public Task EnsureByResourceGuids(HashSet<Guid> resourceGuids) => Task.CompletedTask;
    public Task EnsureByGuids(HashSet<Guid> guids) => Task.CompletedTask;
}