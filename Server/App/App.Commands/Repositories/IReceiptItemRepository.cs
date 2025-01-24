namespace App.Commands.Repositories;

using App.Commands.Base;
using Domain.Entities.Warehouse.Receipt;

public interface IReceiptItemRepository : Item.IRepository, IRepository
{
    public abstract Task FillByMeasureUnitGuids(List<Guid> unitGuids);
    public abstract Task FillByResourceGuids(List<Guid> resourceGuids);
    public abstract Task FillByReceiptGuids(List<Guid> receiptGuids);
}
