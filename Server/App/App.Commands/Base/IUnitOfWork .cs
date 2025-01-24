namespace App.Commands.Base;

using App.Base.Mediator;
using App.Commands.Repositories;
using Domain.Base;

public interface IUnitOfWork : IUnitOFWorkBase, IData
{
    new public IClientRepository Client { get; }
    new public IMeasureUnitRepository MeasureUnit { get; }
    new public IResourceRepository Resource { get; }
    new public IBalanceRepository Balance { get; }
    new public IReceiptRepository Receipt { get; }
    new public IReceiptItemRepository ReceiptItem { get; }
    new public IShipmentRepository Shipment { get; }
    new public IShipmentItemRepository ShipmentItem { get; }
}
