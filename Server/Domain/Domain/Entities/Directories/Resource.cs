namespace Domain.Entities.Directories;

using Common.Exceptions;
using Domain.Base;
using System;
using System.Threading.Tasks;

/// <summary>
/// Ресурс
/// </summary>
public sealed class Resource : BaseEntity
{
    public interface IRepository : IBaseRepository<Resource>
    {
        protected static Resource Restore(Guid guid, string name, Conditions condition)
            => new Resource(guid, name, condition);

        public Task EnsureByNames(HashSet<string> names);
    }

    public string Name { get; private set; }
    public Conditions Condition { get; private set; }

    public enum Conditions
    {
        Work = 1,
        Archive = 2
    }

    private Resource(Guid guid, string name, Conditions condition)
    {
        Guid = guid;
        Name = name;
        Condition = condition;
    }

    public static async Task<List<Resource>> CreateRange(HashSet<string> names, IData data)
    {
        await data.Resources.EnsureByNames(names);

        if (data.Resources.List.Any(x => names.Contains(x.Name)))
            throw new DomainException("В системе уже зарегистрирован ресурс с таким наименованием");

        List<Resource> resources = new List<Resource>();

        foreach (var name in names)
        {
            var resource = new Resource(Guid.CreateVersion7(), name, Conditions.Work);
            resource.Create();
			data.Resources.Add(resource);
			resources.Add(resource);
        }

        return resources;
    }

    public record UpdateArg(Guid Guid, string Name);
    public static async Task UpdateRange(List<UpdateArg> args, IData data)
    {
        var guids = args.Select(x => x.Guid).ToHashSet();
        await data.Resources.EnsureByGuids(guids);

        var names = args.Select(x => x.Name).ToHashSet();
        await data.Resources.EnsureByNames(names);

        var resources = data.Resources.List.Where(x => guids.Contains(x.Guid)).ToList();

        foreach (var resource in resources)
        {
            var arg = args.First(x => x.Guid == resource.Guid);

            resource.Name = arg.Name;
            resource.Update();
        }

        foreach (var arg in args)
        {
            if (data.Resources.List.Any(x => x.Name == arg.Name && x.Guid != arg.Guid))
                throw new DomainException("В системе уже зарегистрирован ресурс с таким наименованием");
        }
    }

    public static async Task DeleteRange(HashSet<Guid> guids, IData data)
    {
        await data.ReceiptItems.EnsureByResourceGuids(guids);
        await data.Balances.EnsureByResourceGuids(guids);
        await data.ShipmentItems.EnsureByResourceGuids(guids);

        var receiptItems = data.ReceiptItems.List.Where(x => guids.Contains(x.ResourceGuid));
        var balances = data.Balances.List.Where(x => guids.Contains(x.ResourceGuid));
        var shipmentItems = data.ShipmentItems.List.Where(x => guids.Contains(x.ResourceGuid));

        if (receiptItems.Any() || balances.Any() || shipmentItems.Any())
            throw new DomainException("Невозможно удалить ресурс, так как он используется");

        await data.Resources.EnsureByGuids(guids);
        var resources = data.Resources.List.Where(x => guids.Contains(x.Guid)).ToList();

        foreach (var resource in resources)
            resource.Remove();
    }

    public static async Task ToArchiveRange(HashSet<Guid> guids, IData data)
    {
        await data.Resources.EnsureByGuids(guids);
        var resources = data.Resources.List.Where(x => guids.Contains(x.Guid)).ToList();

        foreach (var resource in resources)
        {
            if (resource.Condition == Conditions.Work)
            {
                resource.Condition = Conditions.Archive;
                resource.Update();
            }
            else
                throw new DomainException("Невозможно перевести в архив, т.к. ресурс уже находится в архиве");
        }
    }

    public static async Task ToWorkRange(HashSet<Guid> guids, IData data)
    {
        await data.Resources.EnsureByGuids(guids);
        var resources = data.Resources.List.Where(x => guids.Contains(x.Guid)).ToList();

        foreach (var resource in resources)
        {
            if (resource.Condition == Conditions.Archive)
            {
                resource.Condition = Conditions.Work;
                resource.Update();
            }
            else
                throw new DomainException("Невозможно перевести в работу, т.к. ресурс уже находится в работе");
        }
    }
}
