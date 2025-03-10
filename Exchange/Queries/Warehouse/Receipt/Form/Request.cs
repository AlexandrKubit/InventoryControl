namespace Exchange.Queries.Warehouse.Receipt.Form;



public class Request : IRequest<Model>
{
    public Guid Guid { get; set; }
}
