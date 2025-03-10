namespace Exchange.Commands.Warehouse.Shipment.Delete;



public class Request : IRequest<Guid>
{
    public Guid Guid { get; set; }
}
