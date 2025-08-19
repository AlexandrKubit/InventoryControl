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
        protected static Document Restore(Guid id, string name, DateTime date)
            => new Document(id, name, date);

        public Task FillByNumbers(List<string> numbers);
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
        var numbers = args.Select(x => x.Number).ToList();
        await data.Receipt.FillByNumbers(numbers);

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

    public record UpdateArg(Guid Guid, string Number, DateTime Date);
    public static async Task UpdateRange(List<UpdateArg> args, IData data)
    {
        var guids = args.Select(x => x.Guid).Distinct().ToList();
        await data.Receipt.FillByGuids(guids);

        var numbers = args.Select(x => x.Number).Distinct().ToList();
        await data.Receipt.FillByNumbers(numbers);

        var receipts = data.Receipt.List.Where(x => guids.Contains(x.Guid)).ToList();

        foreach (var receipt in receipts)
        {
            var arg = args.First(x => x.Guid == receipt.Guid);

            receipt.Number = arg.Number;
            receipt.Date = arg.Date;
            receipt.Update();
        }

        foreach (var arg in args)
        {
            if (data.Receipt.List.Any(x => x.Number == arg.Number && x.Guid != arg.Guid))
                throw new DomainException("В системе уже зарегистрирована накладная с таким номером");
        }
    }

    // удаление накладной это тоже отдельное бизнес действие, но при этом необходимо удалить и все элементы накладной
    // иначе данные будут в невалидном состоянии
    // на этом примере можно увидеть разницу между бизнес действием и сценарием
    // при том, что удаление ресурса из накладной тоже отдельное бизнесс действие
    public static async Task DeleteRange(List<Guid> receiptGuids, IData data)
    {
        await data.Receipt.FillByGuids(receiptGuids);
        await data.ReceiptItem.FillByReceiptGuids(receiptGuids);

        var itemGuids = data.ReceiptItem.List
            .Where(x => receiptGuids.Contains(x.ReceiptGuid))
            .Select(x=> x.Guid)
            .ToList();

        await Item.DeleteRange(itemGuids, data);

        var receipts = data.Receipt.List.Where(x => receiptGuids.Contains(x.Guid)).ToList();
        foreach (var receipt in receipts)
        {
            receipt.Remove();
        }
    }
}
