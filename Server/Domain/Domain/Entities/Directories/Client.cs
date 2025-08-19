namespace Domain.Entities.Directories;

using Common.Exceptions;
using Domain.Base;
using System;
using System.Threading.Tasks;

/// <summary>
/// Клиент
/// </summary>
public sealed class Client : BaseEntity
{
    public interface IRepository : IBaseRepository<Client>
    {
        protected static Client Restore(Guid guid, string name, string address, Conditions condition)
            => new(guid, name, address, condition);

        public Task FillByNames(List<string> names);
    }


    public string Name { get; private set; }
    public string Address { get; private set; }
    public Conditions Condition { get; private set; }

    public enum Conditions
    {
        Work = 1,
        Archive = 2
    }

    private Client(Guid guid, string name, string address, Conditions condition)
    {
        Guid = guid;
        Name = name;
        Address = address;
        Condition = condition;
    }


    // для улучшения производительности в сложных сценариях имеет смысл создавать методы так
    // чтобы они сразу же умели работать с массивом данных, и воспринимать Mhetod как частынй случай MhetodRange
    public record CreateArg(string Name, string Address);
    public static async Task<List<Client>> CreateRange(List<CreateArg> args, IData data)
    {
        var names = args.Select(x => x.Name).ToList();
        await data.Client.FillByNames(names);

        if (data.Client.List.Any(x => names.Contains(x.Name)))
            throw new DomainException("В системе уже зарегистрирован клиент с таким наименованием");

        List<Client> clients = new List<Client>();

        foreach (var arg in args)
        {
            var client = new Client(Guid.CreateVersion7(), arg.Name, arg.Address, Conditions.Work);
            client.Append(data.Client);
            clients.Add(client);
        }

        return clients;
    }

    public record UpdateArg(Guid Guid, string Name, string Address);
    public static async Task UpdateRange(List<UpdateArg> args, IData data)
    {
        var guids = args.Select(x => x.Guid).Distinct().ToList();
        await data.Client.FillByGuids(guids);

        var names = args.Select(x => x.Name).Distinct().ToList();
        await data.Client.FillByNames(names);

        var clients = data.Client.List.Where(x => guids.Contains(x.Guid)).ToList();

        foreach (var client in clients)
        {
            var arg = args.First(x => x.Guid == client.Guid);

            client.Name = arg.Name;
            client.Address = arg.Address;
            client.Update();
        }

        foreach (var arg in args)
        {
            if (data.Client.List.Any(x => x.Name == arg.Name && x.Guid != arg.Guid))
                throw new DomainException("В системе уже зарегистрирован клиент с таким наименованием");
        }
    }

    public static async Task DeleteRange(List<Guid> guids, IData data)
    {
        await data.Shipment.FillByClients(guids);
        var shipments = data.Shipment.List.Where(x => guids.Contains(x.ClientGuid));

        if (shipments.Any())
            throw new DomainException("Невозможно удалить клиента, т.к. в системе существует отгрузка, использующая его");

        await data.Client.FillByGuids(guids);
        var clients = data.Client.List.Where(x => guids.Contains(x.Guid)).ToList();

        foreach (var client in clients)
            client.Remove();
    }

    public static async Task ToArchiveRange(List<Guid> guids, IData data)
    {
        await data.Client.FillByGuids(guids);
        var clients = data.Client.List.Where(x => guids.Contains(x.Guid)).ToList();

        foreach (var client in clients)
        {
            if (client.Condition == Conditions.Work)
            {
                client.Condition = Conditions.Archive;
                client.Update();
            }
            else
                throw new DomainException("Невозможно перевести в архив, т.к. клиент уже находится в архиве");
        }
    }

    public static async Task ToWorkRange(List<Guid> guids, IData data)
    {
        await data.Client.FillByGuids(guids);
        var clients = data.Client.List.Where(x => guids.Contains(x.Guid)).ToList();

        foreach (var client in clients)
        {
            if (client.Condition == Conditions.Archive)
            {
                client.Condition = Conditions.Work;
                client.Update();
            }
            else
                throw new DomainException("Невозможно перевести в работу, т.к. клиент уже находится в работе");
        }
    }
}
