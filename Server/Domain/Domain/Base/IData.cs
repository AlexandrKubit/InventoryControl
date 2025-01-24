namespace Domain.Base;

using E = Entities;

public interface IData
{
    E.Directories.Client.IRepository Client { get; }
    E.Directories.MeasureUnit.IRepository MeasureUnit { get; }
    E.Directories.Resource.IRepository Resource { get; }
    E.Warehouse.Balance.IRepository Balance { get; }
    E.Warehouse.Receipt.Document.IRepository Receipt { get; }
    E.Warehouse.Receipt.Item.IRepository ReceiptItem { get; }
    E.Warehouse.Shipment.Document.IRepository Shipment { get; }
    E.Warehouse.Shipment.Item.IRepository ShipmentItem { get; }
}
