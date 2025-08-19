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

        public Task FillByNames(List<string> names);
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

    public static async Task<List<Resource>> CreateRange(List<string> names, IData data)
    {
        await data.Resource.FillByNames(names);

        if (data.Resource.List.Any(x => names.Contains(x.Name)))
            throw new DomainException("В системе уже зарегистрирован ресурс с таким наименованием");

        List<Resource> resources = new List<Resource>();

        foreach (var name in names)
        {
            var resource = new Resource(Guid.CreateVersion7(), name, Conditions.Work);
            resource.Append(data.Resource);
            resources.Add(resource);
        }

        return resources;
    }

    public record UpdateArg(Guid Guid, string Name);
    public static async Task UpdateRange(List<UpdateArg> args, IData data)
    {
        var guids = args.Select(x => x.Guid).Distinct().ToList();
        await data.Resource.FillByGuids(guids);

        var names = args.Select(x => x.Name).Distinct().ToList();
        await data.Resource.FillByNames(names);

        var resources = data.Resource.List.Where(x => guids.Contains(x.Guid)).ToList();

        foreach (var resource in resources)
        {
            var arg = args.First(x => x.Guid == resource.Guid);

            resource.Name = arg.Name;
            resource.Update();
        }

        foreach (var arg in args)
        {
            if (data.Resource.List.Any(x => x.Name == arg.Name && x.Guid != arg.Guid))
                throw new DomainException("В системе уже зарегистрирован ресурс с таким наименованием");
        }
    }

    public static async Task DeleteRange(List<Guid> guids, IData data)
    {
        await data.ReceiptItem.FillByResourceGuids(guids);
        await data.Balance.FillByResourceGuids(guids);
        await data.ShipmentItem.FillByResourceGuids(guids);

        var receiptItems = data.ReceiptItem.List.Where(x => guids.Contains(x.ResourceGuid));
        var balances = data.Balance.List.Where(x => guids.Contains(x.ResourceGuid));
        var shipmentItems = data.ShipmentItem.List.Where(x => guids.Contains(x.ResourceGuid));

        if (receiptItems.Any() || balances.Any() || shipmentItems.Any())
            throw new DomainException("Невозможно удалить ресурс, так как он используется");

        await data.Resource.FillByGuids(guids);
        var resources = data.Resource.List.Where(x => guids.Contains(x.Guid)).ToList();

        foreach (var resource in resources)
            resource.Remove();
    }

    public static async Task ToArchiveRange(List<Guid> guids, IData data)
    {
        await data.Resource.FillByGuids(guids);
        var resources = data.Resource.List.Where(x => guids.Contains(x.Guid)).ToList();

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

    public static async Task ToWorkRange(List<Guid> guids, IData data)
    {
        await data.Resource.FillByGuids(guids);
        var resources = data.Resource.List.Where(x => guids.Contains(x.Guid)).ToList();

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
