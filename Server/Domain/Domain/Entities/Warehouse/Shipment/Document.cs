namespace Domain.Entities.Warehouse.Shipment;

using Common.Exceptions;
using Domain.Base;
using System.Threading.Tasks;

/// <summary>
/// Документ отгрузки
/// </summary>
public sealed class Document : BaseEntity
{
    public interface IRepository : IBaseRepository<Document>
    {
        protected static Document Restore(Guid guid, string number, Guid clientGuid, DateTime date, Conditions condition)
            => new Document(guid, number, clientGuid, date, condition);

        public abstract Task EnsureByNumbers(HashSet<string> numbers);
        public abstract Task EnsureByClients(HashSet<Guid> clientGuids);
    }

    // при подписи и отзыве отгрузки необходимо изменять кол-во ресурсов на складе
    // ресурсы на складе просто так нельзя удалять, добавлять или изменять на складе
    // поэтому баланс подписывается на эти события
    #region Events
    public record SignedRangeArg(List<Document> Documents, IData Data);
    public static Action<Func<SignedRangeArg, Task>> OnSignedRange => SignedRange.Subscribe;
    private static readonly DomainEvent<SignedRangeArg> SignedRange = new(); // один подписчик - баланс на складе, порядок не важен  

    public record UnsignedRangeArg(List<Document> Documents, IData Data);
    public static Action<Func<UnsignedRangeArg, Task>> OnUnsignedRange => UnsignedRange.Subscribe;
    private static readonly DomainEvent<UnsignedRangeArg> UnsignedRange = new(); // один подписчик - баланс на складе, порядок не важен  
    #endregion

    public enum Conditions
    {
        Unsigned = 1,
        Signed = 2
    }

    public string Number { get; private set; }
    public Guid ClientGuid { get; private set; }
    public DateTime Date { get; private set; }
    public Conditions Condition { get; private set; }

    private Document(Guid guid, string number, Guid clientGuid, DateTime date, Conditions condition)
    {
        Guid = guid;
        Number = number;
        ClientGuid = clientGuid;
        Date = date;
        Condition = condition;
    }

    // предположим, что в нашем домене нельзя создать пустой документ отгрузки
    // в этом случае нам необходимо в это бизнес действие передать информацию 
    // о ресурсах, которые мы отгружаем
    public record CreateArg(
        string Number, 
        Guid ClientGuid, 
        DateTime Date, 
        List<Item.CreateArg> CreateItems
    );
    public static async Task<List<Document>> CreateRange(List<CreateArg> args, IData data)
    {
        if (args.Any(x => x.CreateItems.Count == 0))
            throw new DomainException("Невозможно создать пустую отгрузку");

        var numbers = args.Select(x => x.Number).ToHashSet();
        await data.Shipments.EnsureByNumbers(numbers);

        if (data.Shipments.List.Any(x => numbers.Contains(x.Number)))
            throw new DomainException(
                "В системе уже зарегистрирована отгрузка с таким номером"
            );

        List<Document> documents = new List<Document>();
        List<Item.CreateArg> createItemArgs = new List<Item.CreateArg>();

        foreach (var arg in args)
        {
            var document = new Document(
                Guid.CreateVersion7(), 
                arg.Number, 
                arg.ClientGuid, 
                arg.Date, 
                Conditions.Unsigned
            );
            document.Create();
			data.Shipments.Add(document);
			documents.Add(document);

            var itemCreateArg = arg.CreateItems.Select(x => 
                new Item.CreateArg(
                    document.Guid,
                    x.ResourceGuid,
                    x.MeasureUnitGuid,
                    x.Quantity
                ));
            createItemArgs.AddRange(itemCreateArg);
        }

        // Если Document.CreateRange нужны сами объекты Item
        // (например, для дальнейшей логики)
        var createdItems = await Item.CreateRange(createItemArgs, data);

        return documents;
    }

    public record UpdateArg(Guid Guid, string Number, Guid ClientGuid, DateTime Date);
    public static async Task UpdateRange(List<UpdateArg> args, IData data)
    {
        var guids = args.Select(x => x.Guid).ToHashSet();
        await data.Shipments.EnsureByGuids(guids);
        var documents = data.Shipments.List.Where(x => guids.Contains(x.Guid)).ToList();

        if (documents.Any(x => x.Condition == Conditions.Signed))
            throw new DomainException("Невозможно отредактировать подписанную отгрузку");

        var numbers = args.Select(x => x.Number).ToHashSet();
        await data.Shipments.EnsureByNumbers(numbers);

        foreach (var document in documents)
        {
            var arg = args.FirstOrDefault(x => x.Guid == document.Guid);

            document.Number = arg.Number;
            document.Date = arg.Date;
            document.ClientGuid = arg.ClientGuid;
            document.Update();
        }

        foreach (var arg in args)
        {
            if (data.Shipments.List.Any(x => x.Number == arg.Number && x.Guid != arg.Guid))
                throw new DomainException("В системе уже зарегистрирована отгрузка с таким номером");
        }
    }

    public static async Task DeleteRange(HashSet<Guid> guids, IData data)
    {
        await data.Shipments.EnsureByGuids(guids);
        var documents = data.Shipments.List.Where(x => guids.Contains(x.Guid)).ToList();

        if (documents.Any(x => x.Condition == Conditions.Signed))
            throw new DomainException("Невозможно удалить подписанную отгрузку");

        await data.ShipmentItems.EnsureByShipmentGuids(guids);

        var itemGuids = data.ShipmentItems.List.Where(x => guids.Contains(x.ShipmentGuid)).Select(x => x.Guid).ToHashSet();
        await Item.DeleteRange(itemGuids, data);

        foreach (var document in documents)
        {
            document.Remove();
        }
    }

    public static async Task SignRange(HashSet<Guid> guids, IData data)
    {
        await data.Shipments.EnsureByGuids(guids);
        var documents = data.Shipments.List.Where(x => guids.Contains(x.Guid)).ToList();

        foreach (var document in documents)
        {
            if (document.Condition == Conditions.Signed)
                throw new DomainException("Невозможно подписать отгрузку, т.к. она уже подписана");

            document.Condition = Conditions.Signed;
            document.Update();
        }

        await data.ShipmentItems.EnsureByShipmentGuids(guids);
        var items = data.ShipmentItems.List.Where(x => guids.Contains(x.ShipmentGuid)).ToList();

        await SignedRange.Invoke(new SignedRangeArg(documents, data));
    }


    public static async Task UnsignRange(HashSet<Guid> guids, IData data)
    {
        await data.Shipments.EnsureByGuids(guids);
        var documents = data.Shipments.List.Where(x => guids.Contains(x.Guid)).ToList();

        foreach (var document in documents)
        {
            if (document.Condition == Conditions.Unsigned)
                throw new DomainException("Невозможно отозвать отгрузку, т.к. она не подписана");

            document.Condition = Conditions.Unsigned;
            document.Update();
        }

        await UnsignedRange.Invoke(new UnsignedRangeArg(documents, data));
    }
}
