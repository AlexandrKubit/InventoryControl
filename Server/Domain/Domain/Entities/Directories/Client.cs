namespace Domain.Entities.Directories;

using Common.Exceptions;
using Domain.Base;
using System;

/// <summary>
/// Клиент
/// </summary>
public sealed class Client : BaseEntity
{
    public interface IRepository : IBaseRepository<Client>
    {
        protected static Client Restore(Guid guid, string name, string address, Conditions condition)
            => new(guid, name, address, condition);
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
    public static List<Client> CreateRange(List<CreateArg> args, IData data)
    {
        var names = args.Select(x => x.Name).ToList();
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

    public record UpdateArg(Client Client)
    {
        public string Name = Client.Name;
        public string Address = Client.Address;
    };
    public static void UpdateRange(List<UpdateArg> args, IData data)
    {
        foreach (var arg in args)
        {
            arg.Client.Name = arg.Name;
            arg.Client.Address = arg.Address;
            arg.Client.Update();
        }

        foreach (var arg in args)
        {
            if (data.Client.List.Any(x => x.Name == arg.Name && x.Guid != arg.Client.Guid))
                throw new DomainException("В системе уже зарегистрирован клиент с таким наименованием");
        }
    }

    public static void DeleteRange(List<Client> clients, IData data)
    {
        var clientGuids = clients.Select(x => x.Guid).ToList();

        var shipments = data.Shipment.List.Where(x => clientGuids.Contains(x.ClientGuid));
        if (shipments.Any())
            throw new Exception("Невозможно удалить клиента, т.к. в системе существует отгрузка, использующая его");

        foreach (var client in clients)
            client.Remove();
    }

    public void ToArchive()
    {
        if (Condition == Conditions.Work)
        {
            Condition = Conditions.Archive;
            Update();
        }
        else
            throw new DomainException("Невозможно перевести в архив, т.к. клиент уже находится в архиве");
    }

    public void ToWork()
    {
        if (Condition == Conditions.Archive)
        {
            Condition = Conditions.Work;
            Update();
        }
        else
            throw new DomainException("Невозможно перевести в работу, т.к. клиент уже находится в работе");
    }
}
