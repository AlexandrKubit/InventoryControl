namespace Exchange.Queries.Warehouse.Balance.List;
public class Model
{
    public List<Balance> Balances { get; set; }

    public class Balance
    {
        public string ResourceName { get; set; }
        public string MeasureUnitName { get; set; }
        public decimal Quantity { get; set; }
    }
}
