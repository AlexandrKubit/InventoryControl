using Domain.Entities.Directories;
using TestProject.Infrastructure;

namespace Tests.Infrastructure
{
    internal class TestClientRepository : TestBaseRepository<Client>, Client.IRepository
    {
        // по сути это для интеграциооных тестов
        public override void InitData()
        {
            list.Add(Client.IRepository.Restore(Guid.NewGuid(), "Client 1", "Address 1", Client.Conditions.Work));
            list.Add(Client.IRepository.Restore(Guid.NewGuid(), "Client 2", "Address 2", Client.Conditions.Archive));
        }

        // а это для юнит тестов
        public void Add(Guid guid, string name, string address, Client.Conditions condition) =>
            list.Add(Client.IRepository.Restore(guid, name, address, condition));

        public Task FillByGuids(List<Guid> guids) => Task.CompletedTask;
        public Task FillByNames(List<string> names) => Task.CompletedTask;
    }
}