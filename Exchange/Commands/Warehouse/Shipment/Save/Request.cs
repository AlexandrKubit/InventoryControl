namespace Exchange.Commands.Warehouse.Shipment.Save;



public class Request : IRequest<Guid>
{
    public Guid Guid { get; set; }
    public string Number { get; set; }
    public Guid ClientGuid { get; set; }
    public DateTime Date { get; set; }
    public List<Item> Items { get; set; }

    public class Item
    {
        public Guid Guid { get; set; }
        public Guid ShipmentGuid { get; set; }
        public Guid ResourceGuid { get; set; }
        public Guid MeasureUnitGuid { get; set; }
        public decimal Quantity { get; set; }
    }
}
