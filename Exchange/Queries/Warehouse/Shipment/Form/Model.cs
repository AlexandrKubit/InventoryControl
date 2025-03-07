namespace Exchange.Queries.Warehouse.Shipment.Form;
public class Model
{
    public Shipment Document { get; set; } = new Shipment();
    public List<Item> Items { get; set; } = new List<Item>();
    public List<Balance> Balances { get; set; } = new List<Balance>();
    public List<Select> Clients { get; set; } = new List<Select>();
    public List<Select> Resources { get; set; } = new List<Select>();
    public List<Select> MeasureUnits { get; set; } = new List<Select>();

    public class Shipment
    {
        public Guid Guid { get; set; }
        public string Number { get; set; }
        public Guid ClientGuid { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public int Condition { get; set; }
    }

    public class Item
    {
        public Guid Guid { get; set; }
        public Guid ResourceGuid { get; set; }
        public Guid MeasureUnitGuid { get; set; }
        public decimal Quantity { get; set; }
    }

    public class Balance
    {
        public Guid ResourceGuid { get; set; }
        public Guid MeasureUnitGuid { get; set; }
        public decimal Quantity { get; set; }
    }

    public class Select
    {
        public string Name { get; set; }
        public Guid Guid { get; set; }
    }
}
