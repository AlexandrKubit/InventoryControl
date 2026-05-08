using Domain.Entities.Directories;
using System.Net.Sockets;
using TestProject.Infrastructure;
using static Domain.Entities.Directories.Client;
namespace Tests.Infrastructure;

internal class TestClientRepository : TestBaseRepository<Client>, Client.IRepository
{
    // для интеграциооных тестов
    public override void InitData()
    {
		Add(Guid.NewGuid(), "Client 1", "Address 1", Client.Conditions.Work);
		Add(Guid.NewGuid(), "Client 2", "Address 2", Client.Conditions.Archive);
    }

    // для юнит тестов
    public void Add(Guid guid, string name, string address, Conditions condition)
    {
        collection.Add(
            guid, 
            Client.IRepository.Restore(guid, name, address, condition)
        );
    }

    public Task EnsureByGuids(HashSet<Guid> guids) => Task.CompletedTask;
    public Task EnsureByNames(HashSet<string> names) => Task.CompletedTask;
}