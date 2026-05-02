namespace Domain.Base;

using E = Entities;

// по сути это UoW, но на уровне домена это всего лишь "умные" коллекции данных 
// можно воспринимать IData как личная песочница для запроса
public interface IData
{
    E.Directories.Client.IRepository Clients { get; }
    E.Directories.MeasureUnit.IRepository MeasureUnits { get; }
    E.Directories.Resource.IRepository Resources { get; }
    E.Warehouse.Balance.IRepository Balances { get; }
    E.Warehouse.Receipt.Document.IRepository Receipts { get; }
    E.Warehouse.Receipt.Item.IRepository ReceiptItems { get; }
    E.Warehouse.Shipment.Document.IRepository Shipments { get; }
    E.Warehouse.Shipment.Item.IRepository ShipmentItems { get; }
}
