using Domain.Entities.Warehouse.Receipt;

namespace TestProject.Infrastructure.Repositories;

internal class TestReceiptItemRepository : TestBaseRepository<Item>, Item.IRepository
{
    // по сути это для интеграциооных тестов
    public override void InitData()
    {
    }

    // а это для юнит тестов
    public void Add(Guid guid, Guid receiptGuid, Guid resourceGuid, Guid measureUnitGuid, decimal quantity) =>
        list.Add(Item.IRepository.Restore(guid, receiptGuid, resourceGuid, measureUnitGuid, quantity));

    public Task FillByMeasureUnitGuids(List<Guid> unitGuids) => Task.CompletedTask;
    public Task FillByResourceGuids(List<Guid> resourceGuids) => Task.CompletedTask;
    public Task FillByReceiptGuids(List<Guid> receiptGuids) => Task.CompletedTask;
    public Task FillByGuids(List<Guid> guids) => Task.CompletedTask;
}
