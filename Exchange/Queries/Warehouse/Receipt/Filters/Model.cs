namespace Exchange.Queries.Warehouse.Receipt.Filters;
public class Model
{
    public List<string> Numbers { get; set; }
    public List<Select> Resources { get; set; }
    public List<Select> MeasureUnits { get; set; }

    public class Select
    {
        public string Name { get; set; }
        public Guid Guid { get; set; }
    }
}
