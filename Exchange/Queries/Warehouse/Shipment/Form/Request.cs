namespace Exchange.Queries.Warehouse.Shipment.Form;



public class Request : IRequest<Model>
{
    public Guid Guid { get; set; }
}
