namespace Domain.Entities.Warehouse.Receipt;

using Common.Exceptions;
using Domain.Base;
using System.Threading.Tasks;

/// <summary>
/// Документ поступления ресурсов (накладная)
/// </summary>
public sealed class Document : BaseEntity
{
    public interface IRepository : IBaseRepository<Document>
    {
        protected static Document Restore(Guid id, string number, DateTime date)
            => new Document(id, number, date);

        public Task EnsureByNumbers(HashSet<string> numbers);
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
    public static async Task<List<Document>> CreateRange(List<CreateArg> args, IData data)
    {
        var numbers = args.Select(x => x.Number).ToHashSet();
        await data.Receipts.EnsureByNumbers(numbers);

        if (data.Receipts.List.Any(x => numbers.Contains(x.Number)))
            throw new DomainException("В системе уже зарегистрирована накладная с таким номером");

        List<Document> documents = new List<Document>();

        foreach (var arg in args)
        {
            var document = new Document(Guid.CreateVersion7(), arg.Number, arg.Date);
            document.Create();
            data.Receipts.Add(document);
			documents.Add(document);
        }

        return documents;
    }

    public record UpdateArg(Guid Guid, string Number, DateTime Date);
    public static async Task UpdateRange(List<UpdateArg> args, IData data)
    {
        var guids = args.Select(x => x.Guid).ToHashSet();
        await data.Receipts.EnsureByGuids(guids);

        var numbers = args.Select(x => x.Number).ToHashSet();
        await data.Receipts.EnsureByNumbers(numbers);

        var receipts = data.Receipts.List.Where(x => guids.Contains(x.Guid)).ToList();

        foreach (var receipt in receipts)
        {
            var arg = args.First(x => x.Guid == receipt.Guid);

            receipt.Number = arg.Number;
            receipt.Date = arg.Date;
            receipt.Update();
        }

        foreach (var arg in args)
        {
            if (data.Receipts.List.Any(x => x.Number == arg.Number && x.Guid != arg.Guid))
                throw new DomainException("В системе уже зарегистрирована накладная с таким номером");
        }
    }

    // удаление накладной это тоже отдельное бизнес действие, но при этом необходимо удалить и все элементы накладной
    // иначе данные будут в невалидном состоянии
    // на этом примере можно увидеть разницу между бизнес действием и сценарием
    // при том, что удаление ресурса из накладной тоже отдельное бизнесс действие
    public static async Task DeleteRange(HashSet<Guid> receiptGuids, IData data)
    {
        await data.Receipts.EnsureByGuids(receiptGuids);
        await data.ReceiptItems.EnsureByReceiptGuids(receiptGuids);

        var itemGuids = data.ReceiptItems.List
            .Where(x => receiptGuids.Contains(x.ReceiptGuid))
            .Select(x=> x.Guid)
            .ToHashSet();

        await Item.DeleteRange(itemGuids, data);

        var receipts = data.Receipts.List.Where(x => receiptGuids.Contains(x.Guid)).ToList();
        foreach (var receipt in receipts)
        {
            receipt.Remove();
        }
    }
}
