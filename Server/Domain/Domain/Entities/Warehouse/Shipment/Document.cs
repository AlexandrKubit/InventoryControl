namespace Domain.Entities.Warehouse.Shipment;

using Domain.Base;
using Common.Exceptions;

/// <summary>
/// Документ отгрузки
/// </summary>
public sealed class Document : BaseEntity
{
    public interface IRepository : IBaseRepository<Document>
    {
        protected static Document Restore(Guid guid, string number, Guid clientGuid, DateTime date, Conditions condition)
            => new Document(guid, number, clientGuid, date, condition);
    }

    // при подписи и отзыве отгрузки необходимо изменять кол-во ресурсов на складе
    // ресурсы на складе просто так нельзя удалять, добавлять или изменять на складе
    // поэтому эти методы приватны и мы передаем их тем сущностям, которые имеют право изменять кол-во ресурсов на складе
    #region delegates
    private static Action<List<Balance.AddRangeToStockArg>, IData> addRangeToStock;
    private static Action<List<Balance.RemoveRangeFromStockArg>, IData> removeRangeFromStock;

    public static void SetAddRangeToStock(Action<List<Balance.AddRangeToStockArg>, IData> action) 
        => addRangeToStock ??= action;
    public static void SetRemoveRangeFromStock(Action<List<Balance.RemoveRangeFromStockArg>, IData> action) 
        => removeRangeFromStock ??= action;
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
    // в этом случае нам необходимо в это бизнес действие передать информацию о ресурсах, которые мы отгружаем
    public record CreateArg(string Number, Guid ClientGuid, DateTime Date, List<Item.CreateArg> CreateItems);
    public static List<Document> CreateRange(List<CreateArg> args, IData data)
    {
        if (args.Any(x => x.CreateItems.Count == 0))
            throw new DomainException("Невозможно создать пустую отгрузку");

        var numbers = args.Select(x => x.Number).ToList();
        
        if (data.Shipment.List.Any(x => numbers.Contains(x.Number)))
            throw new DomainException("В системе уже зарегистрирована отгрузка с таким номером");

        List<Document> documents = new List<Document>();
        List<Item.CreateArg> createItems = new List<Item.CreateArg>();

        foreach (var arg in args)
        {
            var document = new Document(Guid.CreateVersion7(), arg.Number, arg.ClientGuid, arg.Date, Conditions.Unsigned);
            document.Append(data.Shipment);
            documents.Add(document);
            createItems.AddRange(arg.CreateItems.Select(x => new Item.CreateArg(document.Guid, x.ResourceGuid, x.MeasureUnitGuid, x.Quantity)));
        }

        Item.CreateRange(createItems, data);

        return documents;
    }

    public record UpdateArg(Document Document)
    {
        public string Number = Document.Number;
        public Guid ClientGuid = Document.ClientGuid;
        public DateTime Date = Document.Date;
    };
    public static void UpdateRange(List<UpdateArg> args, IData data)
    {
        if (args.Any(x => x.Document.Condition == Conditions.Signed))
            throw new DomainException("Невозможно отредактировать подписанную отгрузку");

        foreach (var arg in args)
        {
            arg.Document.Number = arg.Number;
            arg.Document.Date = arg.Date;
            arg.Document.ClientGuid = arg.ClientGuid;
            arg.Document.Update();
        }

        foreach (var arg in args)
        {
            if (data.Shipment.List.Any(x => x.Number == arg.Number && x.Guid != arg.Document.Guid))
                throw new DomainException("В системе уже зарегистрирована отгрузка с таким номером");
        }
    }

    public static void DeleteRange(List<Document> shipments, IData data)
    {
        if (shipments.Any(x => x.Condition == Conditions.Signed))
            throw new DomainException("Невозможно удалить подписанную отгрузку");

        var shipmentGuids = shipments.Select(x => x.Guid).ToList();       
        var items = data.ShipmentItem.List.Where(x => shipmentGuids.Contains(x.ShipmentGuid)).ToList();
        Item.DeleteRange(items, data);

        foreach (var shipment in shipments)
        {
            shipment.Remove();
        }
    }

    public static void SignRange(List<Document> shipments, IData data)
    {
        foreach (var shipment in shipments)
        {
            if (shipment.Condition == Conditions.Signed)
                throw new DomainException("Невозможно подписать отгрузку, т.к. она уже подписана");

            shipment.Condition = Conditions.Signed;
            shipment.Update();
        }

        var shipmentGuids = shipments.Select(x => x.Guid).ToList();      
        var items = data.ShipmentItem.List.Where(x => shipmentGuids.Contains(x.ShipmentGuid)).ToList();

        List<Balance.RemoveRangeFromStockArg> removeRangeFromStockArgs = items
            .Select(x => new Balance.RemoveRangeFromStockArg(x.ResourceGuid, x.MeasureUnitGuid, x.Quantity))
            .ToList();

        removeRangeFromStock.Invoke(removeRangeFromStockArgs, data);
    }

    public static void UnsignRange(List<Document> shipments, IData data)
    {
        foreach (var shipment in shipments)
        {
            if (shipment.Condition == Conditions.Unsigned)
                throw new DomainException("Невозможно отозвать отгрузку, т.к. она не подписана");

            shipment.Condition = Conditions.Unsigned;
            shipment.Update();
        }

        var shipmentGuids = shipments.Select(x => x.Guid).ToList();
        //await Data.Uow.ShipmentItem.FillByShipmentGuids(shipmentGuids);
        var items = data.ShipmentItem.List.Where(x => shipmentGuids.Contains(x.ShipmentGuid)).ToList();

        List<Balance.AddRangeToStockArg> addRangeToStockArgs = items
            .Select(x => new Balance.AddRangeToStockArg(x.ResourceGuid, x.MeasureUnitGuid, x.Quantity))
            .ToList();

        addRangeToStock.Invoke(addRangeToStockArgs, data);
    }
}
