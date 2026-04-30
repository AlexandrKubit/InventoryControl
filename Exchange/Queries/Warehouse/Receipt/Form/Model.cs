namespace Exchange.Queries.Warehouse.Receipt.Form;
public class Model
{
    public Receipt Document { get; set; } = new Receipt();
    public List<Item> Items { get; set; } = new List<Item>();
    public List<Select> Resources { get; set; } = new List<Select>();
    public List<Select> MeasureUnits { get; set; } = new List<Select>();

    public class Receipt
    {
        public Guid Guid { get; set; }
        public string Number { get; set; }
        public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    }

    public class Item
    {
        public Guid Guid { get; set; }
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
