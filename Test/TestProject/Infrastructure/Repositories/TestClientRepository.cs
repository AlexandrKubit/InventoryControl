using Domain.Entities.Directories;
using TestProject.Infrastructure;

namespace Tests.Infrastructure
{
    internal class TestClientRepository : TestBaseRepository<Client>, Client.IRepository
    {
        public TestClientRepository()
        {
            list.Add(Client.IRepository.Restore(Guid.NewGuid(), "Client 1", "Address 1", Client.Conditions.Work));
            list.Add(Client.IRepository.Restore(Guid.NewGuid(), "Client 2", "Address 2", Client.Conditions.Archive));
        }

        public Task FillByGuids(List<Guid> guids) => Task.CompletedTask;
        public Task FillByNames(List<string> names) => Task.CompletedTask;
    }
}