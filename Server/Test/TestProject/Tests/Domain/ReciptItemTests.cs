using Domain.Entities.Warehouse.Receipt;
using TestProject.Infrastructure.Repositories;
using Tests.Infrastructure;

namespace TestProject.Tests.Domain
{
    public class ReciptItemTests
    {
        [Fact]
        public async Task DeleteReceiptItem_ShouldDecreaseBalance()
        {
            var uow = new TestUnitOfWork();

            var resourceGuid = Guid.CreateVersion7();
            var measureUnitGuid = Guid.CreateVersion7();

            ((TestBalanceRepository)uow.Balance)
                .Add(Guid.CreateVersion7(), resourceGuid, measureUnitGuid, 100);

            var guid = Guid.CreateVersion7();
            ((TestReceiptItemRepository)uow.ReceiptItem)
                .Add(guid, Guid.CreateVersion7(), resourceGuid, measureUnitGuid, 30);

            await Item.DeleteRange([guid], uow);

            var updatedBalance = uow.Balance.List.First();
            Assert.Equal(70, updatedBalance.Quantity);
        }
    }
}
