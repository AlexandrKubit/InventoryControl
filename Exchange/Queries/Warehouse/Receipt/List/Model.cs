namespace Exchange.Queries.Warehouse.Receipt.List;
public class Model
{
    public List<Receipt> Receipts { get; set; } = new List<Receipt>();
    public List<Item> Items { get; set; } = new List<Item>();

    public class Receipt
    {
        public Guid Guid { get; set; }
        public string Number { get; set; }
        public DateTime Date { get; set; }
    }

    public class Item
    {
        public Guid ReceiptGuid { get; set; }
        public string ResourceName { get; set; }
        public string MeasureUnitName { get; set; }
        public decimal Quantity { get; set; }
    }
}
