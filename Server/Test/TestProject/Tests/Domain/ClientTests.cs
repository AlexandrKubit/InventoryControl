using Common.Exceptions;
using Domain.Entities.Directories;
using Tests.Infrastructure;

namespace TestProject.Tests.Domain
{
    public class ClientTests
    {
        // грубо говоря это интеграционный тест
        [Fact]
        public async Task CreateRange_Integrte()
        {
            // Arrange
            var testUow = new TestUnitOfWork();
            testUow.LoadData();
            var newClientArgs = new Client.CreateArg("Unique Name", "New Address");

            // Act
            var result = await Client.CreateRange(new List<Client.CreateArg> { newClientArgs }, testUow);

            // Assert
            Assert.Single(result); // Проверяем, что создан один клиент
            Assert.Equal(3, testUow.Client.List.Count()); // 2 предзаполненных + 1 новый
        }

        // грубо говоря это юнит тест
        [Fact]
        public async Task CreateRange_Unit()
        {
            // Arrange
            var testUow = new TestUnitOfWork();
            ((TestClientRepository)testUow.Client).Add(Guid.NewGuid(), "Client 1", "Address 1", Client.Conditions.Work);
            ((TestClientRepository)testUow.Client).Add(Guid.NewGuid(), "Client 2", "Address 2", Client.Conditions.Work);

            var newClientArgs = new Client.CreateArg("Unique Name", "New Address");

            // Act
            var result = await Client.CreateRange(new List<Client.CreateArg> { newClientArgs }, testUow);

            // Assert
            Assert.Single(result); // Проверяем, что создан один клиент
            Assert.Equal(3, testUow.Client.List.Count()); // 2 предзаполненных + 1 новый
        }

        // реальный тест бизнес-логики
        [Fact]
        public async Task CreateRange_WithExistingName_ThrowsDomainException()
        {
            // Arrange
            var testUow = new TestUnitOfWork();
            ((TestClientRepository)testUow.Client).Add(Guid.NewGuid(), "Existing Name", "Address", Client.Conditions.Work);
            var duplicateClientArg = new Client.CreateArg("Existing Name", "New Address");

            // Act & Assert
            await Assert.ThrowsAsync<DomainException>(() =>
                Client.CreateRange(new List<Client.CreateArg> { duplicateClientArg }, testUow)
            );
        }
    }
}