using Domain.Entities.Warehouse.Receipt;

namespace TestProject.Infrastructure.Repositories;

internal class TestReceiptItemRepository : TestBaseRepository<Item>, Item.IRepository
{
    public override void InitData()
    {
    }

    public void Add(Guid guid, Guid receiptGuid, Guid resourceGuid, Guid measureUnitGuid, decimal quantity) =>
        collection.Add(guid, Item.IRepository.Restore(guid, receiptGuid, resourceGuid, measureUnitGuid, quantity));

    public Task EnsureByMeasureUnitGuids(HashSet<Guid> unitGuids) => Task.CompletedTask;
    public Task EnsureByResourceGuids(HashSet<Guid> resourceGuids) => Task.CompletedTask;
    public Task EnsureByReceiptGuids(HashSet<Guid> receiptGuids) => Task.CompletedTask;
    public Task EnsureByGuids(HashSet<Guid> guids) => Task.CompletedTask;
}
