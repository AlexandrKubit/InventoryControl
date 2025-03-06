namespace Exchange.Queries.Warehouse.Shipment.List;
public class Model
{
    public List<Shipment> Shipments { get; set; } = new List<Shipment>();
    public List<Item> Items { get; set; } = new List<Item>();

    public class Shipment
    {
        public Guid Guid { get; set; }
        public string Number { get; set; }
        public string ClientName { get; set; }
        public DateTime Date { get; set; }
    }

    public class Item
    {
        public Guid ShipmentGuid { get; set; }
        public string ResourceName { get; set; }
        public string MeasureUnitName { get; set; }
        public decimal Quantity { get; set; }
    }
}
