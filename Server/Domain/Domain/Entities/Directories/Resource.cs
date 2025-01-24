namespace Domain.Entities.Directories;

using Domain.Base;
using Common.Exceptions;

/// <summary>
/// Ресурс
/// </summary>
public sealed class Resource : BaseEntity
{
    public interface IRepository : IBaseRepository<Resource>
    {
        protected static Resource Restore(Guid guid, string name, Conditions condition)
            => new Resource(guid, name, condition);
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

    public static List<Resource> CreateRange(List<string> names, IData data)
    {
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

    public record UpdateArg(Resource Resource)
    {
        public string Name = Resource.Name;
    };
    public static void UpdateRange(List<UpdateArg> args, IData data)
    {
        foreach (var arg in args)
        {
            arg.Resource.Name = arg.Name;
            arg.Resource.Update();
        }

        foreach (var arg in args)
        {
            if (data.Resource.List.Any(x => x.Name == arg.Name && x.Guid != arg.Resource.Guid))
                throw new DomainException("В системе уже зарегистрирован ресурс с таким наименованием");
        }
    }

    public static void DeleteRange(List<Resource> resources, IData data)
    {
        var resourceGuids = resources.Select(x => x.Guid).ToList();

        var receiptItems = data.ReceiptItem.List.Where(x => resourceGuids.Contains(x.ResourceGuid));
        var balances = data.Balance.List.Where(x => resourceGuids.Contains(x.ResourceGuid));
        var shipmentItems = data.ShipmentItem.List.Where(x => resourceGuids.Contains(x.ResourceGuid));

        if (receiptItems.Any() || balances.Any() || shipmentItems.Any())
            throw new Exception("Невозможно удалить ресурс, так как он используется");

        foreach (var resource in resources)
            resource.Remove();
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
