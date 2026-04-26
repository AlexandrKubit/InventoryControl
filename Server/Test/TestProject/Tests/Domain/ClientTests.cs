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
            var uow = new TestUnitOfWork();
            uow.LoadData();
            var newClientArgs = new Client.CreateArg("Unique Name", "New Address");

            var result = await Client.CreateRange(new List<Client.CreateArg> { newClientArgs }, uow);

            Assert.Single(result); // Проверяем, что создан один клиент
            Assert.Equal(3, uow.Client.List.Count()); // 2 предзаполненных + 1 новый
        }

        // грубо говоря это юнит тест
        [Fact]
        public async Task CreateRange_Unit()
        {
            var uow = new TestUnitOfWork();
            ((TestClientRepository)uow.Client).Add(Guid.NewGuid(), "Client 1", "Address 1", Client.Conditions.Work);
            ((TestClientRepository)uow.Client).Add(Guid.NewGuid(), "Client 2", "Address 2", Client.Conditions.Work);

            var newClientArgs = new Client.CreateArg("Unique Name", "New Address");

            var result = await Client.CreateRange(new List<Client.CreateArg> { newClientArgs }, uow);

            Assert.Single(result); // Проверяем, что создан один клиент
            Assert.Equal(3, uow.Client.List.Count()); // 2 предзаполненных + 1 новый
        }

        // реальный тест бизнес-логики
        [Fact]
        public async Task CreateRange_WithExistingName_ThrowsDomainException()
        {
            var uow = new TestUnitOfWork();
            ((TestClientRepository)uow.Client).Add(Guid.NewGuid(), "Existing Name", "Address", Client.Conditions.Work);
            var duplicateClientArg = new Client.CreateArg("Existing Name", "New Address");

            await Assert.ThrowsAsync<DomainException>(() =>
                Client.CreateRange(new List<Client.CreateArg> { duplicateClientArg }, uow)
            );
        }
    }
}