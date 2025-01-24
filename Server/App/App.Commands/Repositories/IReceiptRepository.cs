namespace App.Commands.Repositories;

using App.Commands.Base;
using Domain.Entities.Warehouse.Receipt;

public interface IReceiptRepository : Document.IRepository, IRepository
{
    public Task FillByNumbers(List<string> numbers);
}
