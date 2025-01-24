namespace Domain.Entities.Warehouse.Receipt;

using Domain.Base;
using Common.Exceptions;

/// <summary>
/// Документ поступления ресурсов (накладная)
/// </summary>
public sealed class Document : BaseEntity
{
    public interface IRepository : IBaseRepository<Document>
    {
        protected static Document Restore(Guid id, string name, DateTime date)
            => new Document(id, name, date);
    }


    public string Number { get; private set; }
    public DateTime Date { get; private set; }

    private Document(Guid guid, string number, DateTime date)
    {
        Guid = guid;
        Number = number;
        Date = date;
    }

    // предположим, что в нашей предметной области можно создать пустую накладную
    // в этом случае создание накладной это отдельное бизнес дествие
    // тогда как сценарий (команда) создания накладной будет выгядеть как последовательность вызовов бизнес действий:
    // создать накладную, добавть в нее элементы
    public record CreateArg(string Number, DateTime Date);
    public static List<Document> CreateRange(List<CreateArg> args, IData data)
    {
        var numbers = args.Select(x => x.Number).ToList();

        if (data.Receipt.List.Any(x => numbers.Contains(x.Number)))
            throw new DomainException("В системе уже зарегистрирована накладная с таким номером");

        List<Document> documents = new List<Document>();

        foreach (var arg in args)
        {
            var document = new Document(Guid.CreateVersion7(), arg.Number, arg.Date);
            document.Append(data.Receipt);
            documents.Add(document);
        }

        return documents;
    }

    public record UpdateArg(Document Document)
    {
        public string Number = Document.Number;
        public DateTime Date = Document.Date;
    };
    public static void UpdateRange(List<UpdateArg> args, IData data)
    {
        foreach (var arg in args)
        {
            arg.Document.Number = arg.Number;
            arg.Document.Date = arg.Date;
            arg.Document.Update();
        }

        foreach (var arg in args)
        {
            if (data.Receipt.List.Any(x => x.Number == arg.Number && x.Guid != arg.Document.Guid))
                throw new DomainException("В системе уже зарегистрирована накладная с таким номером");
        }
    }

    // удаление накладной это тоже отдельное бизнес действие, но при этом необходимо удалить и все элементы накладной
    // иначе данные будут в невалидном состоянии
    // на этом примере можно увидеть разницу между бизнес действием и сценарием
    // при том, что удаление ресурса из накладной тоже отдельное бизнесс действие
    public static void DeleteRange(List<Document> receipts, IData data)
    {
        var receiptGuids = receipts.Select(x => x.Guid).ToList();
        
        var items = data.ReceiptItem.List.Where(x => receiptGuids.Contains(x.ReceiptGuid)).ToList();
        Item.DeleteRange(items, data);

        foreach (var receipt in receipts)
        {
            receipt.Remove();
        }
    }
}
