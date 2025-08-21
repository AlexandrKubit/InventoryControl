using Domain.Base;
using Domain.Entities.Directories;
using Domain.Entities.Warehouse;

namespace Tests.Infrastructure
{
    internal class TestUnitOfWork : IData
    {
        public TestUnitOfWork()
        {
            Client = new TestClientRepository();
        }

        public Client.IRepository Client { get; }
        public MeasureUnit.IRepository MeasureUnit { get; }
        public Resource.IRepository Resource { get; }
        public Balance.IRepository Balance { get; }
        public Domain.Entities.Warehouse.Receipt.Document.IRepository Receipt { get; }
        public Domain.Entities.Warehouse.Receipt.Item.IRepository ReceiptItem { get; }
        public Domain.Entities.Warehouse.Shipment.Document.IRepository Shipment { get; }
        public Domain.Entities.Warehouse.Shipment.Item.IRepository ShipmentItem { get; }
    }
}