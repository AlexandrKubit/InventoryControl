using Domain.Entities.Directories;
using Tests.Infrastructure;

namespace TestProject.Tests.Domain
{
    public class ClientTests
    {
        [Fact]
        public async Task CreateRange_WithUniqueName_ShouldCreateClient()
        {
            // Arrange
            var testUow = new TestUnitOfWork();
            var newClientArgs = new Client.CreateArg("Unique Name", "New Address");

            // Act
            var result = await Client.CreateRange(new List<Client.CreateArg> { newClientArgs }, testUow);

            // Assert
            Assert.Single(result); // Проверяем, что создан один клиент
            Assert.Equal(3, testUow.Client.List.Count()); // 2 предзаполненных + 1 новый
        }
    }
}