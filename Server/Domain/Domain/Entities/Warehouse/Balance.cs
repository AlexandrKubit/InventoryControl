namespace Domain.Entities.Warehouse;

using Common.Exceptions;
using Domain.Base;
using Domain.Entities.Warehouse.Receipt;

/// <summary>
/// Баланс (свободный остаток на склада)
/// </summary>
public sealed class Balance : BaseEntity
{
    public interface IRepository : IBaseRepository<Balance>
    {
        protected static Balance Restore(Guid guid, Guid resourceGuid, Guid measureUnitGuid, decimal quantity)
            => new Balance(guid, resourceGuid, measureUnitGuid, quantity);
    }


    // мы передаем наши приватные методы только тем сущностям, которые имеют право изменять кол-во ресурсов на складе
    // этот статический конструктор, как и остальные статические конструкторы BaseEntity, вызовется в момент инициализации приложения
    static Balance()
    {
        Item.SetAddRangeToStock(AddRangeToStock);
        Item.SetRemoveRangeFromStock(RemoveRangeFromStock);

        Shipment.Document.SetAddRangeToStock(AddRangeToStock);
        Shipment.Document.SetRemoveRangeFromStock(RemoveRangeFromStock);
    }

    public Guid ResourceGuid { get; }
    public Guid MeasureUnitGuid { get; }
    public decimal Quantity { get; private set; }

    private Balance(Guid guid, Guid resourceGuid, Guid measureUnitGuid, decimal quantity)
    {
        Guid = guid;
        ResourceGuid = resourceGuid;
        MeasureUnitGuid = measureUnitGuid;
        Quantity = quantity;
    }

    public record AddRangeToStockArg(Guid ResourceGuid, Guid MeasureUnitGuid, decimal Quantity);
    private static void AddRangeToStock(List<AddRangeToStockArg> args, IData data)
    {   
        foreach (var arg in args)
        {
            var balance = data.Balance.List.FirstOrDefault(x => x.MeasureUnitGuid == arg.MeasureUnitGuid && x.ResourceGuid == arg.ResourceGuid);
            if (balance != null)
            {
                balance.Quantity += arg.Quantity;
                balance.Update();
            }
            else
            {
                balance = new Balance(Guid.CreateVersion7(), arg.ResourceGuid, arg.MeasureUnitGuid, arg.Quantity);
                balance.Append(data.Balance);
            }
        }
    }

    public record RemoveRangeFromStockArg(Guid ResourceGuid, Guid MeasureUnitGuid, decimal Quantity);
    private static void RemoveRangeFromStock(List<RemoveRangeFromStockArg> args, IData data)
    {
        foreach (var arg in args)
        {
            var balance = data.Balance.List.FirstOrDefault(x => x.MeasureUnitGuid == arg.MeasureUnitGuid && x.ResourceGuid == arg.ResourceGuid);
            if (balance != null)
            {
                if (balance.Quantity < arg.Quantity)
                    throw new DomainException("На складе не достаточно ресурсов");

                balance.Quantity -= arg.Quantity;
                if (balance.Quantity > 0)
                    balance.Update();
                else
                    balance.Remove();
            }
            else
            {
                throw new DomainException("На складе отсутствуют ресурсы");
            }
        }
    }
}
