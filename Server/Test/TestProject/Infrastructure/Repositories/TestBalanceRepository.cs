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
        list.Add(Balance.IRepository.Restore(guid, resourceGuid, measureUnitGuid, quantity));

    public Task FillByResourceMeasureUnit(IEnumerable<(Guid ResourceGuid, Guid MeasureUnitGuid)> args) => Task.CompletedTask;
    public Task FillByMeasureUnitGuids(List<Guid> unitGuids) => Task.CompletedTask;
    public Task FillByResourceGuids(List<Guid> resourceGuids) => Task.CompletedTask;
    public Task FillByGuids(List<Guid> guids) => Task.CompletedTask;
}