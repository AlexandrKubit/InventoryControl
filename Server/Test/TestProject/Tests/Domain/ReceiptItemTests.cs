using Domain.Entities.Warehouse.Receipt;
using TestProject.Infrastructure.Repositories;
using Tests.Infrastructure;

namespace TestProject.Tests.Domain
{
    public class ReceiptItemTests
    {
        [Fact]
        public async Task DeleteReceiptItem_ShouldDecreaseBalance()
        {
            var data = new TestData();

            var resourceGuid = Guid.CreateVersion7();
            var measureUnitGuid = Guid.CreateVersion7();

            ((TestBalanceRepository)data.Balances)
                .Add(Guid.CreateVersion7(), resourceGuid, measureUnitGuid, 100);

            var guid = Guid.CreateVersion7();
            ((TestReceiptItemRepository)data.ReceiptItems)
                .Add(guid, Guid.CreateVersion7(), resourceGuid, measureUnitGuid, 30);

            await Item.DeleteRange([guid], data);

            var updatedBalance = data.Balances.List.First();
            Assert.Equal(70, updatedBalance.Quantity);
        }
    }
}
